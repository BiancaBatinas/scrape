using CefSharp;
using CefSharp.WinForms;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapingEcap.models
{
    public class JsConnection
    {
        private readonly ChromiumWebBrowser chromeBrowser;

        public JsConnection(ChromiumWebBrowser browser)
        {
            chromeBrowser = browser;
        }

        public string SelectedElementTag { get; set; }

        public void ElementClicked(string tag)
        {
            SelectedElementTag = tag;

            // Execute JavaScript code to update the ComboBox
            string script = $"window.boundObject.UpdateComboBox('{tag}');";
            chromeBrowser.ExecuteScriptAsync(script);
        }
    }
}
