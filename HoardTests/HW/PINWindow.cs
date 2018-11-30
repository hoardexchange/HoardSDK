using System.Threading;
using System.Windows.Forms;

namespace HoardTests.HW
{
    public partial class PINWindow : Form
    {
        public string PINValue { get; private set; }

        public ManualResetEvent PINEnteredEvent;

        public PINWindow()
        {
            InitializeComponent();
            PINEnteredEvent = new ManualResetEvent(false);
        }

        private void PinEnter(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                PINValue = pinBox.Text;
                PINEnteredEvent.Set();
                this.Hide();
            }
        }
    }
}
