using Xunit;

public class MatKhauTests
{
    private static readonly string Salt16 = Convert.ToBase64String(new byte[16]);
    private static readonly string Bam32 = Convert.ToBase64String(new byte[32]);


    [Fact]
    public void CungMatKhauBamHaiLan_RaHaiKetQuaKhacNhau()
    {
        var a = MatKhau.Tao("Abc@12345");
        var b = MatKhau.Tao("Abc@12345");

        Assert.NotEqual(a.Salt, b.Salt);   // salt ngẫu nhiên
        Assert.NotEqual(a.Bam, b.Bam);     // nên hash cũng khác
    }

    [Fact]
    public void XacThucDung_TraVeTrue()
    {
        var (salt, bam) = MatKhau.Tao("Abc@12345");
        Assert.True(MatKhau.KiemTra("Abc@12345", salt, bam));
    }

    [Fact]
    public void XacThucSai_TraVeFalse()
    {
        var (salt, bam) = MatKhau.Tao("Abc@12345");
        Assert.False(MatKhau.KiemTra("SaiMatKhau", salt, bam));
    }

    

    [Fact]
    public void Tao_SaltVaHashDungDoDai()
    {
        var (salt, bam) = MatKhau.Tao("Abc@12345");
        Assert.Equal(16, Convert.FromBase64String(salt).Length);
        Assert.Equal(32, Convert.FromBase64String(bam).Length);
    }

   

    [Theory]
    [InlineData("@@@", "@@@")]        // không phải Base64
    [InlineData("", "")]
    [InlineData("AAAA", "AAAA")]      // Base64 hợp lệ nhưng sai độ dài
    public void SaltHoacHashHong_TraFalse(string salt, string bam)
    {
        Assert.False(MatKhau.KiemTra("Abc@12345", salt, bam));
    }

    [Fact]
    public void NullTrongDB_TraFalse()
    {
        Assert.False(MatKhau.KiemTra("Abc@12345", null!, null!));
    }

    [Fact]
    public void BamRong_TraFalse()
    {
        Assert.False(MatKhau.KiemTra("bat-ky", Salt16, ""));
    }

    [Fact]
    public void SaltSaiDoDai_TraFalse()
    {
        Assert.False(MatKhau.KiemTra("bat-ky", "AAAA", Bam32));
    }

    [Fact]
    public void BamSaiDoDai_TraFalse()
    {
        Assert.False(MatKhau.KiemTra("bat-ky", Salt16, "AAAA"));
    }

    [Fact]
    public void MatKhauRong_TraFalse()
    {
        var (salt, bam) = MatKhau.Tao("Abc@12345");
        Assert.False(MatKhau.KiemTra("", salt, bam));
    }
}