//heavily modified idea from
//https://social.msdn.microsoft.com/Forums/windows/en-US/97c18a1d-729e-4a68-8223-0fcc9ab9012b/automatically-wrap-text-in-label?forum=winforms
using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;


public class GrowRich : RichTextBox
{
    private const int EM_GETLINECOUNT = 0xba;
    private bool mGrowing;
    public GrowRich()
    {
        ReadOnly = true;
        BorderStyle = 0;
        TabStop = false;
        Multiline = true;
        WordWrap = true;
        //we are a textbox bc you actually cant read mspa without highlighting
        //this.AutoSize = true;
    }
    private void resizeLabel()
    {
        if (mGrowing) return;
        try
        {
            mGrowing = true;
            var numberOfLines = GetLineFromCharIndex(TextLength) + 1;
            this.Height = (Font.Height + 2) * numberOfLines;
        }
        finally
        {
            mGrowing = false;
        }
    }
    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        resizeLabel();
    }
    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        resizeLabel();
    }
    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        resizeLabel();
    }
}

