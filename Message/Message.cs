using System.Windows.Forms;

public class Message
{
    public static void Show(string text, string caption, int buttons = 0, int icon = 16)
    {
        MessageBox.Show(text, caption, (MessageBoxButtons)buttons, (MessageBoxIcon)icon);
    }
}

