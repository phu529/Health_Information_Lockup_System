using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthShared;

// =====================================================================
// BƯỚC 1.3 - ĐÓNG KHUNG THÔNG ĐIỆP BẰNG KÝ TỰ LF (xuống dòng, 0x0A)
//
// Vì sao cần: TCP là dòng byte, không giữ ranh giới tin (xem Chương 3).
//   - "Dính gói": 3 tin gửi liên tiếp có thể tới cùng một lần đọc.
//   - "Phân mảnh": 1 tin có thể tới làm nhiều lần đọc (kể cả cắt giữa một chữ có dấu).
// Quy ước của nhóm: MỖI THÔNG ĐIỆP = MỘT DÒNG JSON UTF-8, KẾT THÚC BẰNG LF.
// Chi tiết xem docs/protocol.md.
//
// Hai lớp trong file này:
//   LfFraming     : GHI một thông điệp (thêm LF ở cuối).
//   LfFrameReader : ĐỌC từng thông điệp, tự ghép mảnh và tự tách gói dính.
// Cả Client và Server dùng chung hai lớp này nên luôn khớp nhau.
// =====================================================================

/// <summary>Thông điệp dài hơn giới hạn 64 KiB. Server trả mã 413 rồi đóng kết nối.</summary>
public sealed class FrameTooLargeException : Exception
{
    public FrameTooLargeException()
        : base($"Thông điệp vượt quá {ProtocolConstants.MaxMessageBytes} byte.") { }
}

/// <summary>Ghi một thông điệp ra luồng: nội dung JSON + một ký tự LF.</summary>
public static class LfFraming
{
    public static async Task WriteAsync(
        Stream stream, string json, CancellationToken token = default)
    {
        // JSON do ProtocolHelper tạo ra luôn escape xuống dòng (\n), nên LF thật không xuất hiện.
        // Kiểm lại để một dòng "lạ" không bị tách thành hai thông điệp.
        if (json.Contains('\n') || json.Contains('\r'))
            throw new ArgumentException("Thông điệp không được chứa ký tự xuống dòng.", nameof(json));

        int soByte = Encoding.UTF8.GetByteCount(json);
        if (soByte > ProtocolConstants.MaxMessageBytes)
            throw new FrameTooLargeException();

        // Gom nội dung + LF vào MỘT mảng rồi ghi một lần: tránh tách làm hai gói TCP nhỏ.
        byte[] goi = new byte[soByte + 1];
        Encoding.UTF8.GetBytes(json, 0, json.Length, goi, 0);
        goi[soByte] = (byte)'\n';

        await stream.WriteAsync(goi.AsMemory(), token).ConfigureAwait(false);
        await stream.FlushAsync(token).ConfigureAwait(false);
    }
}

/// <summary>
/// Đọc từng thông điệp (một dòng kết thúc bằng LF) từ luồng TCP.
/// Mỗi kết nối dùng MỘT đối tượng riêng vì nó giữ bộ đệm phần dữ liệu đã nhận nhưng chưa đủ dòng.
/// Không an toàn khi hai tác vụ cùng gọi ReadLineAsync trên một đối tượng (mỗi kết nối chỉ có một vòng đọc).
/// </summary>
public sealed class LfFrameReader
{
    private readonly Stream _stream;
    private byte[] _buf = new byte[8192];
    private int _start;   // vị trí byte đầu tiên chưa xử lý
    private int _end;     // vị trí sau byte cuối cùng đã nhận

    public LfFrameReader(Stream stream) => _stream = stream;

    /// <summary>
    /// Trả về nội dung một thông điệp (đã bỏ LF và CR đứng trước nếu có).
    /// Trả về null khi bên kia đóng kết nối đàng hoàng (không còn dữ liệu dở dang).
    /// Dòng trống được trả về là chuỗi rỗng - người gọi nên bỏ qua.
    /// Ném FrameTooLargeException nếu một dòng dài quá 64 KiB, EndOfStreamException nếu kết nối đóng khi đang nhận dở một dòng.
    /// </summary>
    public async Task<string?> ReadLineAsync(CancellationToken token = default)
    {
        int quetTu = _start;   // chỉ tìm LF trong phần byte mới, không quét lại phần cũ

        while (true)
        {
            // 1) Trong bộ đệm đã có đủ một dòng chưa? (trường hợp DÍNH GÓI: có thể có nhiều dòng)
            int viTriLf = Array.IndexOf(_buf, (byte)'\n', quetTu, _end - quetTu);
            if (viTriLf >= 0)
            {
                int doDai = viTriLf - _start;
                if (doDai > 0 && _buf[viTriLf - 1] == (byte)'\r') doDai--;   // chấp nhận CRLF (netcat, telnet)

                if (doDai > ProtocolConstants.MaxMessageBytes)
                    throw new FrameTooLargeException();

                string dong = Encoding.UTF8.GetString(_buf, _start, doDai);
                _start = viTriLf + 1;
                if (_start == _end) _start = _end = 0;   // hết dữ liệu: quay về đầu bộ đệm
                return dong;
            }

            // 2) Chưa đủ dòng (trường hợp PHÂN MẢNH): kiểm giới hạn rồi đọc thêm.
            if (_end - _start > ProtocolConstants.MaxMessageBytes + 1)
                throw new FrameTooLargeException();

            quetTu = _end;

            if (_end == _buf.Length)
            {
                if (_start > 0)
                {
                    // Dồn dữ liệu chưa xử lý về đầu bộ đệm để lấy chỗ trống.
                    int daDoi = _start;
                    Buffer.BlockCopy(_buf, _start, _buf, 0, _end - _start);
                    _end -= daDoi;
                    _start = 0;
                    quetTu -= daDoi;
                }
                else
                {
                    // Một dòng dài hơn bộ đệm: mở rộng (có trần, tránh bị ép tốn bộ nhớ).
                    Array.Resize(ref _buf, Math.Min(_buf.Length * 2, ProtocolConstants.MaxMessageBytes + 8192));
                }
            }

            int n = await _stream.ReadAsync(_buf.AsMemory(_end), token).ConfigureAwait(false);
            if (n == 0)
            {
                if (_end > _start)
                    throw new EndOfStreamException("Kết nối đóng khi đang nhận dở một thông điệp.");
                return null;   // đóng đàng hoàng
            }
            _end += n;
        }
    }
}
