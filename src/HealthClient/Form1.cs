using HealthShared;

namespace HealthClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Text = $"He thong tra cuu trieu chung (khung) - cong {ProtocolConstants.DefaultTcpPort}";
        }
    }
}
