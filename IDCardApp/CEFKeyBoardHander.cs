using CefSharp;
using System.Windows.Forms;

namespace IDCardApp
{
    public class CEFKeyBoardHander : IKeyboardHandler
    {
        public bool OnKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
        {
            var code = windowsKeyCode;
            if (type == KeyType.KeyUp)
            {
                switch (code)
                {
                    case (int)Keys.F5:
                        if (chromiumWebBrowser.Address.StartsWith("asset")) break;
                        browser.Reload();
                        break;
                }

                // Ctrl key down
                if (modifiers == CefEventFlags.ControlDown)
                {
                    switch (code)
                    {
                        // print
                        case (int)Keys.P:
                            browser.Print();
                            break;
                        case (int)Keys.F5:
                            browser.Reload(true);
                            break;
                        case (int)Keys.F12:
                            browser.ShowDevTools();
                            break;

                    }
                }
            }

            return false;
        }

        public bool OnPreKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
        {
            return false;
        }
    }
}
