using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

using CefSharp;
using CefSharp.DevTools.Network;
using CefSharp.Web;
using CefSharp.WinForms;




using CsvHelper.Configuration;

using CsvHelper;

using Newtonsoft.Json;

using OfficeOpenXml;

using WebScrapingEcap.models;

using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using OfficeOpenXml.Style;
using CefSharp.Handler;
using System.Threading;
using System.Security.Policy;
using static WebScrapingEcap.models.Mapari;
using System.Dynamic;

namespace WebScrapingEcap
{
    public partial class Chromium : Form
    {
        public ChromiumWebBrowser chromeBrowser;
        public List<ProductInfo> productsList;
        private List<string> paginationLinks = new List<string>();
        public bool isSelecting = false;
        List<Category> categories = new List<Category>();
        List<ProductInfo> produse = new List<ProductInfo>();

        private ExternalBoundObject externalBoundObject;


        public Chromium()
        {

            InitializeComponent();
            container.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            this.Resize += Form1_Resize;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            btnSave.Enabled = true;
            paginationLinks = new List<string> { "https://fermier.ro" };
            externalBoundObject = new ExternalBoundObject(this);
            InitializeChromium();

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            SetFormSize();
        }

        private void SetFormSize()
        {
            Screen screen = Screen.PrimaryScreen;
            this.Size = new Size(screen.WorkingArea.Width, screen.WorkingArea.Height);
            this.Location = new Point(0, 0);
        }

        private void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            Cef.Initialize(settings);
            chromeBrowser = new ChromiumWebBrowser(paginationLinks[0]);
            chromeBrowser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;

            chromeBrowser.JavascriptObjectRepository.NameConverter = new CefSharp.JavascriptBinding.CamelCaseJavascriptNameConverter();


            var externalBoundObject = new ExternalBoundObject(this);
            chromeBrowser.JavascriptObjectRepository.Register("bind", externalBoundObject, isAsync: false, options: BindingOptions.DefaultBinder);

            chromeBrowser.JavascriptObjectRepository.ObjectBoundInJavascript += (sender, e) =>
            {
                var name = e.ObjectName;
                Debug.WriteLine($"Object {e.ObjectName} was bound successfully.");

            };
            this.Controls.Add(chromeBrowser);
            this.container.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;
            chromeBrowser.AddressChanged += ChromeBrowser_AddressChanged;
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            chromeBrowser.JavascriptMessageReceived += OnBrowserJavascriptMessageReceived;
           




            chromeBrowser.FrameLoadEnd += async (s, args) =>
            {

                if (args.Frame.IsMain)
                {
                    var script2 = @"
        (async function() {
            await CefSharp.BindObjectAsync('bind');
                let selectedElement =null;
                let currentPosition = -1;
                let currentSegment = '';


            myBoundObject = {
                getSelectBtn: function() {
                    return bind.getSelectBtn();
                },
                setSelectBtn: function(select) {
                    bind.setSelectBtn(select);
                },
                getCheckMultiple: function() {
                    return bind.getCheckMultiple();
                },
                setCheckMultiple: function(select) {
                    bind.setCheckMultiple(select);
                },
                onBtnStateChanged: function(newSelectBtnState) {
                    console.log('Starea butonului IsBtnSelectActive actualizată: ' + newSelectBtnState);
                    if (newSelectBtnState) {
                        document.addEventListener('mouseover', function(e) {
                          var target = e.target;
                               if (selectedElement && target !== selectedElement) {
                            target.style.backgroundColor = 'yellow';
                        }
                        });

                       document.addEventListener('mouseout', function(e) {
                            var target = e.target;
                            if (selectedElement && target !== selectedElement) {
        target.style.backgroundColor = '';
    }
                        });
                    }
                },
                onElementClick: function(selectorInfo) {
                        console.log('Elementul a fost apăsat. Selector complet: ' + selectorInfo.fullSelector);
                         console.log('targoviste + ',selectedElement.fullSelector );

                          // Eliminați fundalul anterior (dacă există) de pe elementul anterior
                            if (selectedElement && selectedElement.fullSelector) {
                                document.querySelector(selectedElement.fullSelector).style.backgroundColor = '';
                            }

                            // Setați fundalul elementului curent
                            if (selectorInfo && selectorInfo.fullSelector) {
                                document.querySelector(selectorInfo.fullSelector).style.backgroundColor = 'red';
                            }

                            // Actualizați informațiile elementului selectat
                            selectedElement = selectorInfo;

                            // Actualizați textbox-ul
                            bind.updateTextBox(selectorInfo.finalSelector);
                        

                    },
              btnParinte_Click: function() {
                        if (selectedElement) {
                            const fullSelector = selectedElement.fullSelector;
                            const segments = fullSelector.split(' > ');

                            currentPosition = (currentPosition - 1 + segments.length) % segments.length;
                            let currentSegment = segments[currentPosition];

                            // Verificăm dacă ultimul segment este un tag simplu, cum ar fi <a>
                            if (currentPosition === segments.length - 1 && currentSegment.indexOf(':') === -1) {
                                const previousSegment = segments[currentPosition - 1];
                                currentSegment = previousSegment + ' > ' + currentSegment;
                            }

                            console.log(`Segment la stânga: ${currentSegment}`);
                            bind.updateTextBox(currentSegment);
                            changeBackgroundColorRed(currentSegment);
                        }
                    },

                    btnCopil_Click: function() {
                        if (selectedElement) {
                            const fullSelector = selectedElement.fullSelector;
                            const segments = fullSelector.split(' > ');

                            currentPosition = (currentPosition + 1) % segments.length;
                            let currentSegment = segments[currentPosition];

                            // Verificăm dacă ultimul segment este un tag simplu, cum ar fi <a>
                            if (currentPosition === segments.length - 1 && currentSegment.indexOf(':') === -1) {
                                const previousSegment = segments[currentPosition - 1];
                                currentSegment = previousSegment + ' > ' + currentSegment;
                            }

                            console.log(`Segment la dreapta: ${currentSegment}`);
                            bind.updateTextBox(currentSegment);
                            changeBackgroundColorRed(currentSegment);
                        }
                    }

            };

                function changeBackgroundColorRed(selector) {
                        const selectedElement = document.querySelector(selectedElement.fullSelector);
                        if (selectedElement) {
                            selectedElement.style.backgroundColor = 'red';
                        } else {
                            console.error('Nu a fost selectat niciun element.');
                        }
                    } 

                           function getLastSegment(fullSelector) {
                            if (!fullSelector) return '';

                            const segments = fullSelector.split(' > ');
                            return segments.length > 0 ? segments[segments.length - 1] : '';
                        }


                            function getCssSelectorInfo(element) {
                                if (!element || element.nodeType !== 1) {
                                    return null;
                                }

                                const selectorInfo = {
                                    fullSelector: '',
                                    finalSelector: ''
                                };
                              

                                   
                                     const   fullSelector = getSelector(element);
                                        

                                // Încercăm să eliminăm selectorii părinți
                                const cs = fullSelector.split(' > ');
                                let minSelector = cs.pop();
                                    while (document.querySelectorAll(minSelector).length > 1) {
                                        minSelector = cs.pop() + "" > "" + minSelector;
                                    }


                                selectorInfo.fullSelector = fullSelector;
                                selectorInfo.finalSelector = minSelector;

                                return selectorInfo;
                            }

                            var getSelector = function(el) {
                               if (el.tagName.toLowerCase() === ""html"") {
        return ""html"";
    }
    var str = el.tagName.toLowerCase();
    str += (el.id !== """") ? ""#"" + el.id : """";
    if (el.className) {
        var classes = el.className.trim().split(/\s+/);
        for (var i = 0; i < classes.length; i++) {
            str += ""."" + classes[i];
        }
    }

    // Obținem poziția elementului în lista de frați a părintelui
    var index = Array.from(el.parentNode.children).indexOf(el) + 1;
    str += "":nth-child("" + index + "")"";

    if (document.querySelectorAll(str).length === 1) {
        return str;
    }

    return getSelector(el.parentNode) + "" > "" + str;
                            }

                     
                myBoundObject.onBtnStateChanged(myBoundObject.getSelectBtn());
                    console.log('Checkmultiplu',myBoundObject.getCheckMultiple());

                document.addEventListener('click', function(event) {
                        if (myBoundObject.getSelectBtn()) {
                            event.preventDefault();
                            const clickedElement = event.target;
                            const selectorInfo = getCssSelectorInfo(clickedElement);
                            selectedElement=selectorInfo;
                            if (!clickedElement.hasAttribute('onclick')) {
            // Apelăm metoda onElementClick doar dacă elementul nu are atributul ""onclick""
            myBoundObject.onElementClick(selectorInfo);
        }

                        }
            });
        })();
    ";
                    var script = @"
                                    (async function() {
                                        await CefSharp.BindObjectAsync('bind');
                                        let selectedElement =null;
                                        let currentPosition = -1;
                                        let currentSegment = '';

                                        myBoundObject = {
                                            getSelectBtn: function() {
                                                return bind.getSelectBtn();
                                            },
                                            setSelectBtn: function(select) {
                                                bind.setSelectBtn(select);
                                            },
                                            getCheckMultiple: function() {
                                                return bind.getCheckMultiple();
                                            },
                                            setCheckMultiple: function(select) {
                                                bind.setCheckMultiple(select);
                                            },
                                            onBtnStateChanged: function(newSelectBtnState) {
                                                console.log('Starea butonului IsBtnSelectActive actualizată: ' + newSelectBtnState);
                                                if (newSelectBtnState) {
                                                    document.addEventListener('mouseover', function(e) {
                                                        var target = e.target;
                                                        if (selectedElement && target !== selectedElement) {
                                                            target.style.backgroundColor = 'yellow';
                                                        }
                                                    });
                                                    document.addEventListener('mouseout', function(e) {
                                                        var target = e.target;
                                                        if (selectedElement && target !== selectedElement) {
                                                            target.style.backgroundColor = '';
                                                        }
                                                    });
                                                }
                                            },
                                            onElementClick: function(selectorInfo) {
                                                console.log('Elementul a fost apăsat. Selector complet: ' + selectorInfo.fullSelector);
                                                console.log('targoviste + ',selectedElement.fullSelector );
                                                if (selectedElement && selectedElement.fullSelector) {
                                                    document.querySelector(selectedElement.fullSelector).style.backgroundColor = '';
                                                }
                                                if (selectorInfo && selectorInfo.fullSelector) {
                                                document.querySelector(selectorInfo.fullSelector).style.backgroundColor = 'red';
                                                }
                                                selectedElement = selectorInfo;
                                                bind.updateTextBox(selectorInfo.finalSelector);
                                            },
                                            btnParinte_Click: function() {
                                                if (selectedElement) {
                                                    const fullSelector = selectedElement.fullSelector;
                                                    const segments = fullSelector.split(' > ');
                                                    currentPosition = (currentPosition - 1 + segments.length) % segments.length;
                                                    let currentSegment = segments[currentPosition];
                                                    if (currentPosition === segments.length - 1 && currentSegment.indexOf(':') === -1) {
                                                        const previousSegment = segments[currentPosition - 1];
                                                        currentSegment = previousSegment + ' > ' + currentSegment;
                                                    }
                                                    console.log(`Segment la stânga: ${currentSegment}`);
                                                    bind.updateTextBox(currentSegment);
                                                    changeBackgroundColorRed(currentSegment);
                                                }
                                            },
                                            btnCopil_Click: function() {
                                                if (selectedElement) {
                                                    const fullSelector = selectedElement.fullSelector;
                                                    const segments = fullSelector.split(' > ');
                                                    currentPosition = (currentPosition + 1) % segments.length;
                                                    let currentSegment = segments[currentPosition];
                                                    if (currentPosition === segments.length - 1 && currentSegment.indexOf(':') === -1) {
                                                        const previousSegment = segments[currentPosition - 1];
                                                        currentSegment = previousSegment + ' > ' + currentSegment;
                                                    }
                                                    console.log(`Segment la dreapta: ${currentSegment}`);
                                                    bind.updateTextBox(currentSegment);
                                                    changeBackgroundColorRed(currentSegment);
                                                }
                                            }
                                        };
                                        function changeBackgroundColorRed(selector) {
                                            const selectedElement = document.querySelector(selectedElement.fullSelector);
                                            if (selectedElement) {
                                                selectedElement.style.backgroundColor = 'red';
                                            }
                                            else {
                                                console.error('Nu a fost selectat niciun element.');
                                            }
                                        }
                                        function getLastSegment(fullSelector) {
                                            if (!fullSelector) return '';
                                            const segments = fullSelector.split(' > ');
                                            return segments.length > 0 ? segments[segments.length - 1] : '';
                                        }
                                        function getCssSelectorInfo(element) {
                                            if (!element || element.nodeType !== 1) {
                                                return null;
                                            }
                                            const selectorInfo = {
                                                fullSelector: '',
                                                finalSelector: ''
                                            };
                                            const fullSelector = getSelector(element
                                            const cs = fullSelector.split(' > ');
                                            let minSelector = cs.pop();
                                            while (document.querySelectorAll(minSelector).length > 1) {
                                                minSelector = cs.pop() + "" > "" + minSelector;
                                            }
                                            selectorInfo.fullSelector = fullSelector;
                                            selectorInfo.finalSelector = minSelector;
                                            return selectorInfo;
                                        }
                                        var getSelector = function(el) {
                                            if (el.tagName.toLowerCase() == ""html"")
                                                return ""html"";
                                            var str = el.tagName.toLowerCase();
                                            str += (el.id != """") ? ""#"" + el.id : """";
                                            if (el.className) {
                                                var classes = el.className.trim().split(/\s+/);
                                                for (var i = 0; i < classes.length; i++) {
                                                    str += ""."" + classes[i]
                                                }
                                            }
                                            if(document.querySelectorAll(str).length==1) return str;
                                            return getSelector(el.parentNode) + "" > "" + str;
                                        }

                                           
                                        myBoundObject.onBtnStateChanged(myBoundObject.getSelectBtn());
                                      
                                        
                                      

                                        document.addEventListener('click', function(event) {
                                            console.log('CheckMultipleBtn: ' );
                                            if (myBoundObject.getSelectBtn()) {
                                                event.preventDefault();
                                                const clickedElement = event.target;
                                                const selectorInfo = getCssSelectorInfo(clickedElement);
                                                selectedElement=selectorInfo;
                                                if (!clickedElement.hasAttribute('onclick')) {
                                                    myBoundObject.onElementClick(selectorInfo);
                                                }
                                            }
                                        });
                                    })();
                                  ";
                    var response = await chromeBrowser.EvaluateScriptAsync(script2);

                    if (response.Result != null)
                    {
                        string res = response.Result.ToString();
                        Console.WriteLine("VerificaValue: " + res);
                    }
                }
            };
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // Invertați starea checkbox ului (schimbați între true și false)
            externalBoundObject.SetCheckMultiple(!externalBoundObject.GetCheckMultiple());

            Console.WriteLine("checkbox ! " + externalBoundObject.GetCheckMultiple());

            // Executați scriptul JavaScript pentru a actualiza starea în pagina web
            string script = $"myBoundObject.setCheckMultiple({externalBoundObject.GetCheckMultiple().ToString().ToLower()});";
            chromeBrowser.ExecuteScriptAsync(script);

        }

        private async void button3_Click_1(object sender, EventArgs e)
        {
            // Ia valorile din câmpuri
            string selector = textBox1.Text;
            string url = txtUrl.Text;
            string type = comboType.SelectedItem?.ToString(); 

          
                if (!string.IsNullOrEmpty(selector) && !string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(type))
                {
                    string infoScript2 = $"fetch('{url}')"
                         + ".then(response => response.text())"
                         + ".then(html => new DOMParser().parseFromString(html, 'text/html'))"
                         + ".then(doc => ({ ";

                    switch ((SelectorType)Enum.Parse(typeof(SelectorType), type))
                    {
                        case SelectorType.Text:
                            infoScript2 += $"result: doc.querySelector('{selector}') !== null ? doc.querySelector('{selector}').textContent : null ";
                            break;
                        case SelectorType.Link:
                            infoScript2 += $"result: doc.querySelector('{selector}') !== null ? doc.querySelector('{selector}').href : null ";
                            break;
                        case SelectorType.Image:
                            infoScript2 += $"result: doc.querySelector('{selector}') !== null ? doc.querySelector('{selector}').src : null ";
                            break;
                    case SelectorType.Attribute:
                        {
                            var attribute = selector.Substring(selector.IndexOf('[') + 1, selector.IndexOf(']') - selector.IndexOf('[') - 1);
                            infoScript2 += $"result: Array.from(doc.querySelectorAll('{selector.Split('[')[0]}')).map(atr => atr.getAttribute('{attribute}') || null) ";
                            break;
                        }

                }

                infoScript2 += "}));";


                    var infiResult = await chromeBrowser.EvaluateScriptAsync(infoScript2);
                    var productInformation = infiResult.Success ? infiResult.Result : "N/A";
                // Verificați dacă obiectul returnat este de tipul ExpandoObject
                if (productInformation is ExpandoObject expandoObject)
                {
                    // Iterați prin fiecare pereche cheie-valoare în obiectul ExpandoObject
                    foreach (var kvp in expandoObject)
                    {
                        // Afișați cheia și valoarea asociată
                        Console.WriteLine($"Cheie: {kvp.Key}, Valoare: {kvp.Value}");
                    }
                }
                else
                {
                    // Dacă obiectul returnat nu este de tipul ExpandoObject, tratați-l în consecință
                    Console.WriteLine("Obiectul returnat nu este de tipul ExpandoObject.");
                }
                txtTest.Text = Regex.Replace(productInformation.ToString(), @"[\s\n\r]+", " ").Trim();
                }
            else
            {
                // Afisează un mesaj de avertisment dacă unul sau mai multe câmpuri sunt goale
                MessageBox.Show("Vă rugăm să completați toate câmpurile înainte de a continua.", "Avertisment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void btnParinte_Click(object sender, EventArgs e)
        {
            chromeBrowser.ExecuteScriptAsync("myBoundObject.btnParinte_Click();");
          
        }

        private void btnCopil_Click(object sender, EventArgs e)
        {
            chromeBrowser.ExecuteScriptAsync("myBoundObject.btnCopil_Click();");
           
        }


        private void button3_Click(object sender, EventArgs e)
        {
            // Ia valoarea din textBox1
            string selector = textBox1.Text;
            string id = txtID.Text;
            string type = comboType.GetItemText(comboType.SelectedItem);
            bool isList = checkBox1.Checked;

            // Verifică dacă toate câmpurile sunt completate
            if (!string.IsNullOrEmpty(selector) && !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(type) )
            {
                // Salvează selectorul în lista de selectoare
                externalBoundObject.AddSelectorToList(new SelectorInfo()
                {
                    ID = id,
                    Selector = selector,
                    Type = (SelectorType)Enum.Parse(typeof(SelectorType), type),
                    IsList = isList
                });

                // Șterge textul din câmpuri după adăugare
                textBox1.Clear();
                txtID.Clear();
                checkBox1.Checked = false;
            }
            else
            {
                // Afisează un mesaj de avertisment dacă unul sau mai multe câmpuri sunt goale
                MessageBox.Show("Vă rugăm să completați toate câmpurile înainte de a salva în listă.", "Avertisment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void btnSelect_Click(object sender, EventArgs e)
        {
            // Invertați starea butonului (schimbați între true și false)
            externalBoundObject.SetSelectBtn(!externalBoundObject.GetSelectBtn());

            Console.WriteLine("New select btn state: " + externalBoundObject.GetSelectBtn());

            // Executați scriptul JavaScript pentru a actualiza starea în pagina web
            string script = $"myBoundObject.setSelectBtn({externalBoundObject.GetSelectBtn().ToString().ToLower()});";
            chromeBrowser.ExecuteScriptAsync(script);

            // Reîncărcați pagina pentru a aplica modificările
            chromeBrowser.Reload();
        }

        private void OnBrowserJavascriptMessageReceived(object sender, JavascriptMessageReceivedEventArgs e)
        {
            var windowSelection = (string)e.Message;
            Console.WriteLine("Javascript message received: " + e.Message);

        }

        private async void Chromium_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void ChromeBrowser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                txtUrl.Text = e.Address;
            }));
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            chromeBrowser.Load(txtUrl.Text);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            chromeBrowser.Refresh();
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (chromeBrowser.CanGoForward)
            {
                chromeBrowser.Forward();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (chromeBrowser.CanGoBack)
            {
                chromeBrowser.Back();
            }
        }

        private void Chromium_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();
        }

        private async void btnScrape_Click(object sender, EventArgs e)
        {


            // Disable the Save button
            btnSave.Enabled = false;
            try
            {
                // Show loading form or overlay if needed

               // categories = await ExtractCategoriesAsync();

                // After the process is completed, enable the Save button
                btnSave.Enabled = true;
            }
            catch (Exception ex)
            {
                // Handle exceptions, if any
                Console.WriteLine($"Error: {ex.Message}");
            }



        }

        private async Task<Variants> LoadAvailabilityAsync(ChromiumWebBrowser browser, Variants colorInfo)
        {
            // Execută scriptul pentru a obține valabilitatea
            string availabilityScript = $"fetch('{colorInfo.Link}')"
                                        + ".then(response => response.text())"
                                        + ".then(html => new DOMParser().parseFromString(html, 'text/html'))"
                                         + ".then(doc => ({"
                   + "    Availability: (doc.querySelector('td.param-value.productstock-param') !== null) ? doc.querySelector('td.param-value.productstock-param').textContent : null,"
                   + "    Sku: (doc.querySelector('td.param-value.productsku-param span[itemprop=\"sku\"]') !== null) ? doc.querySelector('td.param-value.productsku-param span[itemprop=\"sku\"]').textContent : null,"
                   + "    Description: (doc.querySelector('\ttd.product-short-description') !== null) ? doc.querySelector('td.product-short-description').textContent : null,"
                   + "}));";

            var availabilityResult = await browser.EvaluateScriptAsync(availabilityScript);
            var productInformation = availabilityResult.Success ? availabilityResult.Result : "N/A";

            var data = JsonConvert.SerializeObject(availabilityResult.Result);
            var da = JsonConvert.DeserializeObject<Variants>(data);

            da.Availability = System.Text.RegularExpressions.Regex.Replace(da.Availability, @"[\s\n\r]+", " ").Trim();
            da.Description = System.Text.RegularExpressions.Regex.Replace(da.Description, @"[\s\n\r]+", " ").Trim();

            colorInfo.Availability = da.Availability;
            colorInfo.Description = da.Description;
            colorInfo.Sku = da.Sku;

            return colorInfo;



        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "XML Files (*.xml)|*.xml|CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*"; saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;

                        switch (Path.GetExtension(filePath).ToLower())
                        {
                            case ".xml":
                                SaveToXml(filePath);
                                break;

                            case ".csv":
                                SaveToCsv2(categories, filePath);
                                break;

                            case ".xlsx":
                                SaveToExcel(categories, filePath);
                                break;

                            default:
                                MessageBox.Show("Formatul selectat nu este suportat.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare în btnSave_Click: {ex.Message}");
            }
        }

        private void SaveToXml(string filePath)
        {
            /* XmlSerializer serializer = new XmlSerializer(typeof(List<ProductInfo>));

             using (TextWriter writer = new StreamWriter(filePath))
             {
                 serializer.Serialize(writer, productsList);
             }
            */

            XmlSerializer serializer = new XmlSerializer(typeof(List<ProductInfo>));

            using (TextWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, produse);
            }

            MessageBox.Show($"Datele au fost salvate cu succes în fișierul {filePath}", "Salvare cu succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveToCsv2(List<Category> categories, string filePath)
        {
            
        }

        private void SaveToExcel(List<Category> categories, string filePath)
        {
           
        }
        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {

            string infoScript = GetScriptFromSelectors(txtUrl.Text, externalBoundObject.listaSelectoare);
            var infiResult = await chromeBrowser.EvaluateScriptAsync(infoScript);
            var productInformation = infiResult.Success ? infiResult.Result : "N/A";
            var data = JsonConvert.SerializeObject(infiResult.Result);
           


        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        /*
         * Creeaza un script dinaic pe baza  selectoarelor salvate in listaSelectoare
         */
        public string GetScriptFromSelectors(string url, List<SelectorInfo> selectors)
        {
            string infoScript = $"fetch('{url}')"
                 + ".then(response => response.text())"
                 + ".then(html => new DOMParser().parseFromString(html, 'text/html'))"
                 + ".then(doc => ({\n";

            foreach (var selector in selectors)
            {
                if (selector.ID == "pagina" || selector.ID == "paginator" || selector.ID == "categorie" || selector.ID == "nrPagini")
                {
                    continue; 
                }

                if (selector.IsList)
                {
                    switch (selector.Type)
                    {
                        case SelectorType.Text:
                            infoScript += $"    {selector.ID}: Array.from(doc.querySelectorAll('{selector.Selector}')).map(text => (text.textContent || null)),\n";
                            break;
                        case SelectorType.Link:
                            infoScript += $"    {selector.ID}: Array.from(doc.querySelectorAll('{selector.Selector}')).map(link => (link.href || null)),\n";
                            break;
                        case SelectorType.Image:
                            infoScript += $"    {selector.ID}: Array.from(doc.querySelectorAll('{selector.Selector}')).map(img => img.src || null),\n";
                            break;
                        case SelectorType.Attribute:
                            var attribute = selector.Selector.Substring(selector.Selector.IndexOf('[') + 1, selector.Selector.IndexOf(']') - selector.Selector.IndexOf('[') - 1);
                            infoScript += $"    {selector.ID}: Array.from(doc.querySelectorAll('{selector.Selector.Split('[')[0]}')).map(atr => atr.getAttribute('{attribute}') || null),\n";
                            break;
                    }
                }
                else
                {
                    switch (selector.Type)
                    {
                        case SelectorType.Text:
                            infoScript += $"    {selector.ID}: doc.querySelector('{selector.Selector}') !== null ? doc.querySelector('{selector.Selector}').textContent : null,\n";
                            break;
                        case SelectorType.Link:
                            infoScript += $"    {selector.ID}: doc.querySelector('{selector.Selector}') !== null ? doc.querySelector('{selector.Selector}').href : null,\n";
                            break;
                        case SelectorType.Image:
                            infoScript += $"    {selector.ID}: doc.querySelector('{selector.Selector}') !== null ? doc.querySelector('{selector.Selector}').src : null,\n";
                            break;
                        case SelectorType.Attribute:
                            var attribute = selector.Selector.Substring(selector.Selector.IndexOf('[') + 1, selector.Selector.IndexOf(']') - selector.Selector.IndexOf('[') - 1);
                            infoScript += $"    {selector.ID}: doc.querySelector('{selector.Selector.Split('[')[0]}') !== null ? doc.querySelector('{selector.Selector.Split('[')[0]}').getAttribute('{attribute}') : null,\n";
                            break;
                    }
                }
            }
            infoScript += "}));";
            return infoScript;
        }

        /*
        * Parcurge toate paginile de la categoria X si creeaza o lista cu toate produsele
        */
        private async Task<List<ProductInfo>> ScrapePages(string url, string category, string subCategory)
        {
            List<ProductInfo> productsTotalPages = new List<ProductInfo>();

            string elementPagina = externalBoundObject.listaSelectoare.FirstOrDefault(x => x.ID == "pagina").Selector;

            var currentPageProducts = await ScrapeProductsFromPage(url, elementPagina, category, subCategory);

            productsTotalPages.AddRange(currentPageProducts);

            string nextPageUrl = null;

            // Extrage numărul paginii din URL-ul curent
            int currentPageNumber = ExtractPageNumberFromUrl(url);

            // Creează link-ul către pagina următoare
            int nextPageNumber = currentPageNumber + 1;

            string nrTotalPagini = externalBoundObject.listaSelectoare.FirstOrDefault(x => x.ID == "nrPagini").Selector;

            string script;
            // Script pentru a găsi numărul total de pagini disponibile pe prima pagină
            if(url.Contains("fermier.ro"))
            {
                script = $"fetch('{url}')"
        +".then(response => response.text())"
        + ".then(html => new DOMParser().parseFromString(html, 'text/html'))"
        + ".then(doc => {"
        + $"    let resultsElement = Array.from(doc.querySelectorAll('{nrTotalPagini}'));"
          + "    let lastText = resultsElement[resultsElement.length - 1].textContent.trim();"
        + "    return parseInt(lastText);"
        + "});";
            }
            else

            {
                 script = $"fetch('{url}')"
              + ".then(response => response.text())"
              + ".then(html => new DOMParser().parseFromString(html, 'text/html'))"
              + ".then(doc => {"
              + $"    let resultsElement = doc.querySelector('{nrTotalPagini}');"
              + "    if (resultsElement) {"
              + "        let matches = resultsElement.textContent.match(/\\((\\d+) pagini\\)/);"
              + "        if (matches) {"
              + "            return parseInt(matches[1]);"
              + "        }"
              + "    }"
              + "    return 1;"  // Întoarce 1 dacă nu găsește informații despre pagini
              + "});";
            }

            var result = await chromeBrowser.EvaluateScriptAsync(script);
            
            // Verifică dacă există un număr total de pagini disponibile
            if (result.Success && result.Result is int totalPages && nextPageNumber <= totalPages)
            {


                if (url.Contains("fermier.ro"))
                {
                    if (!url.Contains("/filtru/ordonare-1/limita-12/pag-"))
                    {
                        nextPageUrl = $"{url}/filtru/ordonare-1/limita-12/pag-{nextPageNumber}";
                    }
                    else
                    {
                        nextPageUrl = Regex.Replace(url, @"\/pag-\d+\/?", $"/pag-{nextPageNumber}");
                    }

                    Console.WriteLine("Accesarea paginii: " + nextPageUrl);
                  
                }
                else
                {

                    // Creează link-ul către pagina următoare
                    if (url.Contains("?"))
                    {
                        // Dacă există, înlocuiește valoarea parametrului "page"
                        nextPageUrl = Regex.Replace(url, @"page=\d+", $"page={nextPageNumber}");
                    }
                    else
                    {
                        // Dacă nu există, adaugă parametrul "page" la URL
                        nextPageUrl = $"{url}?page={nextPageNumber}";
                    }
                }

                Console.WriteLine("Link catre pagina urmatoare: " + nextPageUrl + "--------------------------------");

                // Verifică dacă pagina următoare este validă și diferită de pagina curentă
                if (!string.IsNullOrEmpty(nextPageUrl) && url != nextPageUrl)
                {
                    Console.WriteLine("Pagina: " + url);
                    var nextPageProducts = await ScrapePages(nextPageUrl, category, subCategory);
                    productsTotalPages.AddRange(nextPageProducts);
                }
                else
                {
                    Console.WriteLine("Ultima pagină a fost atinsă.");
                }
            }
            else
            {
                Console.WriteLine("Ultima pagină a fost atinsă sau nu s-a putut găsi numărul total de pagini disponibile.");
            }

            return productsTotalPages;
        }



        private int ExtractPageNumberFromUrl(string url)
        {
            // Verificăm dacă URL-ul aparține site-ului fermieri.ro
            if (url.Contains("fermier.ro"))
            {
                // Dacă URL-ul conține sufixul "/pag-X", unde X este numărul paginii, extragem X
                Match match = Regex.Match(url, @"/pag-(\d+)/?$");

                if (match.Success && int.TryParse(match.Groups[1].Value, out int pageNumber))
                {
                    return pageNumber;
                }
                else
                {
                    // Dacă nu există sufixul în URL sau nu putem extrage numărul, returnăm 1
                    return 1;
                }
            }
            else
            {
                // Dacă nu, procedăm așa cum este definit în codul tău anterior pentru alte site-uri
                var uri = new Uri(url);
                var queryParameters = uri.Query.TrimStart('?').Split('&')
                    .Select(parameter => parameter.Split('='))
                    .ToDictionary(parts => parts[0], parts => parts.Length > 1 ? parts[1] : "");

                if (queryParameters.ContainsKey("page") && int.TryParse(queryParameters["page"], out int pageNumber))
                {
                    return pageNumber;
                }
                else
                {
                    return 1;
                }
            }
        }


        /*
        * Creeaza un obiect productInfo cu toate informatiile de pe pagina unui produs
        */
        private async Task<List<ProductInfo>> ScrapeProductsFromPage(string url, string elementPagina, string category, string subCategory)
        {
            List<ProductInfo> productList = new List<ProductInfo>();

            string infoScript = $"fetch('{url}')"
         + ".then(response => response.text())"
         + ".then(html => new DOMParser().parseFromString(html, 'text/html'))"
         + $".then(doc => Array.from(doc.querySelectorAll('{elementPagina}')).map(link => ({{ ProductName: link.title, ProductLink: link.href }})))";

            var productsListResult = await chromeBrowser.EvaluateScriptAsync(infoScript);
            var productInformation = productsListResult.Success ? productsListResult.Result : "N/A";


            if (productsListResult.Success)
            {
                var data = JsonConvert.SerializeObject(productInformation);
                var jsonObject = JsonConvert.DeserializeObject<List<ProductInfo>>(data);

                if (productsListResult.Success && productsListResult.Result is List<object> products)
                {
                    
                    foreach (var productObj in jsonObject)
                    {
                        Console.WriteLine("Scrape produs: " + productObj.ProductName);

                        ProductInfo productInfo = new ProductInfo
                        {
                            ProductName = productObj.ProductName,
                            ProductLink = productObj.ProductLink,
                            Category = category,
                            SubCategory = subCategory
                        };


                        string yes = GetScriptFromSelectors(productObj.ProductLink, externalBoundObject.listaSelectoare);
                        var infiResult = await chromeBrowser.EvaluateScriptAsync(yes);
                        var k = infiResult.Success ? infiResult.Result : "N/A";

                        if (infiResult.Success)
                        {
                            var expandoObject = infiResult.Result as IDictionary<string, object>;
                            if (expandoObject != null)
                            {
                                foreach (var kvp in expandoObject)
                                {
                                    string key = kvp.Key;
                                    object value = kvp.Value;
                                    Console.WriteLine($"Cheie: {key}, Valoare: {value}");

                                    if (key == "pret" && (value == null ||
                                string.IsNullOrWhiteSpace(value.ToString()) ||
                                !Regex.IsMatch(value.ToString(), @"\d") ||
                                value.ToString().Contains("Acest produs nu este disponibil în stoc.")))
                                    {
                                        value = "1";
                                    }

                                    // Verificăm dacă valoarea este un array
                                    if (value is IEnumerable<object> enumerableValue)
                                    {
                                        if (key != "variante" && key != "numeVarianta" && key != "sku" && key!= "subcategorie")
                                        {
                                            // Convertim fiecare element din array la string și le adăugăm la lista DynamicAttributes
                                            List<string> dynamicValues = enumerableValue
                                             .Where(v => v != null) // Selectăm doar valorile non-nule
                                             .Select(v => v.ToString()) // Convertim valorile non-nule la șiruri de caractere
                                             .ToList();
                                            string formattedList = string.Join(", ", dynamicValues);
                                            productInfo.DynamicAttributes[key] = formattedList;
                                        }

                                        if (kvp.Key == "variante" && kvp.Value != null)
                                        {
                                            if (kvp.Value is List<object> variantLinks && variantLinks.Count > 0)
                                            {
                                                List<string> stringVariantLinks = variantLinks.Select(v => v.ToString()).ToList();

                                                foreach (var variantLink in stringVariantLinks)
                                                {
                                                    string numeVarianta = externalBoundObject.listaSelectoare.FirstOrDefault(x => x.ID == "numeVarianta").Selector;
                                                    string sku = externalBoundObject.listaSelectoare.FirstOrDefault(x => x.ID == "model").Selector;
                                                    string availability = externalBoundObject.listaSelectoare.FirstOrDefault(x => x.ID == "availabilitySelector").Selector;
                                                    string desc = externalBoundObject.listaSelectoare.FirstOrDefault(x => x.ID == "pret").Selector;
                                                    // Apelează funcția LoadAvailabilityAsync pentru a obține informațiile despre disponibilitate, SKU și descriere
                                                    var variantInfo = await LoadAvailabilityAsync(new Variants { Link = variantLink }, numeVarianta, availability, sku, desc);

                                                    // Verifică dacă s-a obținut cu succes informațiile despre variantă
                                                    if (variantInfo != null)
                                                    {
                                                        // Adaugă informațiile despre variantă în lista de obiecte Variants din ProductInfo
                                                        productInfo.Variante.Add(variantInfo);
                                                    }
                                                }
                                            }

                                        }
                                        if (key == "specificatii" )
                                        {
                                           

                                            List<string> dynamicValues = new List<string>();

                                            // Iterăm prin fiecare element al array-ului
                                            foreach (var obj in enumerableValue)
                                            {
                                                // Verificăm dacă obiectul din array este un șir de caractere
                                                if (obj is string stringValue)
                                                {
                                                    // Înlocuim toate aparițiile de "\t" și "\n" cu un spațiu
                                                    stringValue = stringValue.Replace("\n\t", " ");
                                                    stringValue = Regex.Replace(stringValue, @"\s+", " ");
                                                    dynamicValues.Add(stringValue);
                                                    
                                                }
                                            }

                                            // Convertim lista de șiruri într-un șir de caractere și actualizăm valoarea în obiectul ProductInfo
                                            string formattedList = string.Join(", ", dynamicValues);
                                            productInfo.DynamicAttributes[key] = formattedList;
                                        }
                                        if (key == "numeSpecificatii")
                                        {


                                            List<string> dynamicValues = new List<string>();

                                            // Iterăm prin fiecare element al array-ului
                                            foreach (var obj in enumerableValue)
                                            {
                                                // Verificăm dacă obiectul din array este un șir de caractere
                                                if (obj is string stringValue)
                                                {
                                                    // Înlocuim toate aparițiile de "\t" și "\n" cu un spațiu
                                                    stringValue = stringValue.Replace("\n\t", " ");
                                                    stringValue = Regex.Replace(stringValue, @"\s+", " ");
                                                    dynamicValues.Add(stringValue);

                                                }
                                            }

                                            // Convertim lista de șiruri într-un șir de caractere și actualizăm valoarea în obiectul ProductInfo
                                            string formattedList = string.Join(", ", dynamicValues);
                                            productInfo.DynamicAttributes[key] = formattedList;
                                        }

                                    }
                                    else
                                    {
                                        // Dacă valoarea nu este un array, o convertim la string și o adăugăm direct la lista DynamicAttributes
                                        string stringValue = value?.ToString().Trim();
                                        if (key == "availabilitySelector")
                                        {
                                            switch (stringValue)
                                            {
                                                case "In stock":
                                                    productInfo.DynamicAttributes[key] = ((int)StockStatus.InStoc).ToString();
                                                    break;
                                                case "Pre-order (Ask for availability)":
                                                    productInfo.DynamicAttributes[key] = ((int)StockStatus.PreOrderAskForAvailability).ToString();
                                                    break;
                                                default:
                                                    productInfo.DynamicAttributes[key] = stringValue;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            // Verificați dacă cheia este "sku" și asigurați-vă că valoarea este de tip string
                                            if (key == "sku" && value != null)
                                            {
                                                string skuValue = value.ToString(); // Convertim valoarea la string (dacă nu este deja)

                                                // Verificăm dacă șirul începe cu "Cod produs:"
                                                if (skuValue.TrimStart().StartsWith("Cod produs:", StringComparison.OrdinalIgnoreCase)) // Folosim StringComparison.OrdinalIgnoreCase pentru a face compararea nesensibilă la majuscule/minuscule
                                                {
                                                    // Eliminăm "Cod produs:" din începutul șirului, inclusiv spațiile înainte și după
                                                    skuValue = skuValue.Substring(skuValue.IndexOf("Cod produs:", StringComparison.OrdinalIgnoreCase) + "Cod produs:".Length).TrimStart(); // Eliminăm spațiile de la început
                                                                                                                                                                                           // Eliminăm spațiile de la final
                                                    skuValue = skuValue.Trim();

                                                    // Adăugăm valoarea actualizată în lista DynamicAttributes
                                                    productInfo.DynamicAttributes[key] = skuValue;
                                                }
                                            }
                                            else
                                            {

                                                if (key == "stoc" && value != null) // Verificați dacă cheia este "stoc" și asigurați-vă că valoarea este de tip string
                                                {
                                                    string stocValue = value.ToString().Trim().ToLower(); // Convertim șirul la litere mici pentru o verificare mai robustă

                                                    // Verificăm conținutul șirului pentru a stabili stocul
                                                    if (stocValue.Contains("in stoc"))
                                                    {
                                                        productInfo.DynamicAttributes[key] = "In stoc";
                                                    }
                                                    else if (stocValue.Contains("stoc limitat"))
                                                    {
                                                        productInfo.DynamicAttributes[key] = "Stoc limitat";
                                                    }
                                                    else if (stocValue.Contains("indisponibil"))
                                                    {
                                                        productInfo.DynamicAttributes[key] = "Indisponibil";
                                                    }
                                                    else
                                                    {
                                                        // Dacă nu se potrivește cu niciuna dintre opțiuni, păstrăm valoarea originală
                                                        productInfo.DynamicAttributes[key] = stringValue;
                                                    }
                                                }
                                              
                                                else
                                                {
                                                    if(key == "pret" && value != null)
                                                    {
                                                        // Extragem doar cifrele din valoarea prețului
                                                        string pretValue = Regex.Replace(value.ToString(), @"[^0-9.,]", "");
                                                        productInfo.DynamicAttributes[key] = pretValue;
                                                    }
                                                    else if (key == "brand" && value != null)
                                                    {
                                                        // Adăugăm brandul la DynamicAttributes
                                                        productInfo.DynamicAttributes[key] = value.ToString().Trim();
                                                    }
                                                    else
                                                    {
                                                        // Dacă nu este niciuna dintre cheile specifice, păstrăm valoarea originală
                                                        productInfo.DynamicAttributes[key] = stringValue;
                                                    }
                                                }

                                            }
                                        }


                                    }

                                }
                            }


                            if (url.Contains("fermier"))
                            {
                                if (productInfo.DynamicAttributes.ContainsKey("specificatii") && productInfo.DynamicAttributes.ContainsKey("numeSpecificatii"))
                                {
                                    var specificatiiDict = new Dictionary<string, string>(); // Creăm un nou dicționar simplu cu cheie-valoare

                                    // Obținem string-urile delimitate stocate în DynamicAttributes
                                    var specificatiiString = productInfo.DynamicAttributes["specificatii"].ToString();
                                    var numeSpecificatiiString = productInfo.DynamicAttributes["numeSpecificatii"].ToString();

                                    // Verificăm dacă obiectele sunt string-uri
                                    if (!string.IsNullOrEmpty(specificatiiString) && !string.IsNullOrEmpty(numeSpecificatiiString))
                                    {
                                        // Convertim string-urile în liste de șiruri folosind separatorul "\n\t"
                                        var specificatiiList = specificatiiString.Split(new string[] { "\n\t" }, StringSplitOptions.RemoveEmptyEntries)
                                                                                .Select(str => str.Trim())
                                                                                .ToList();
                                        var numeSpecificatiiList = numeSpecificatiiString.Split(new string[] { "\n\t" }, StringSplitOptions.RemoveEmptyEntries)
                                                                                            .Select(str => str.Trim())
                                                                                            .ToList();

                                        if (numeSpecificatiiList.Count == specificatiiList.Count)
                                        {
                                            for (int i = 0; i < numeSpecificatiiList.Count; i++)
                                            {
                                                specificatiiDict[numeSpecificatiiList[i]] = specificatiiList[i];
                                            }


                                            // Adăugăm noul dicționar în DynamicAttributes
                                            productInfo.DynamicAttributes["Specificatii"] = specificatiiDict;

                                            // Ștergem string-urile originale din DynamicAttributes
                                            productInfo.DynamicAttributes.Remove("specificatii");
                                            productInfo.DynamicAttributes.Remove("numeSpecificatii");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Listele specificatii și numeSpecificatii nu au aceeași lungime.");
                                        }
                                    }
                                    else
                                    {
                                        // Dacă șirurile de specificații sunt goale, punem un tag gol <specificatii/>
                                        productInfo.DynamicAttributes["Specificatii"] = string.Empty;
                                        // Ștergem string-urile originale din DynamicAttributes
                                        productInfo.DynamicAttributes.Remove("specificatii");
                                        productInfo.DynamicAttributes.Remove("numeSpecificatii");
                                    }
                                }
                            }

                            productList.Add(productInfo);



                        }

                    }

                    
                }

            }

            return productList;
        }

        private async Task<Variants> LoadAvailabilityAsync( Variants variantInfo,string numeVarianta, string availabilitySelector, string model, string descriptionSelector)
        {
            // Construim scriptul de evaluare folosind selectoarele specificate de utilizator
            string availabilityScript = $"fetch('{variantInfo.Link}')"
                            + ".then(response => response.text())"
                            + ".then(html => new DOMParser().parseFromString(html, 'text/html'))"
                            + ".then(doc => ({"
                            + $"    Name: (doc.querySelector('{numeVarianta}') !== null) ? doc.querySelector('{numeVarianta}').textContent : null,"
                            + $"    Availability: (doc.querySelector('{availabilitySelector}') !== null) ? doc.querySelector('{availabilitySelector}').textContent : null,"
                            + $"    Sku: (doc.querySelector('{model}') !== null) ? doc.querySelector('{model}').textContent : null,"
                            + $"    Description: (doc.querySelector('{descriptionSelector}') !== null) ? doc.querySelector('{descriptionSelector}').textContent : null"
                            + "}));";

            var availabilityResult = await chromeBrowser.EvaluateScriptAsync(availabilityScript);

            if (availabilityResult.Success)
            {
                var data = JsonConvert.SerializeObject(availabilityResult.Result);
                var da = JsonConvert.DeserializeObject<Variants>(data);

                if (da.Availability != null)
                {
                    da.Availability = System.Text.RegularExpressions.Regex.Replace(da.Availability, @"[\s\n\r]+", " ").Trim();

                    switch (da.Availability)
                    {
                        case "In stock":
                            da.Availability = ((int)StockStatus.InStoc).ToString();
                            break;
                        case "Pre-order (Ask for availability)":
                            da.Availability = ((int)StockStatus.PreOrderAskForAvailability).ToString();
                            break;
                        default:
                            da.Availability = da.Availability;
                            break;
                    }
                }

              
                return da;
            }
            else
            {
                return null;
            }
        }

        private async void btnPageScrape_Click(object sender, EventArgs e)
        {
              List<ProductInfo> produse =new List<ProductInfo>();
            categories = new List<Category>();
              string selector_categorie = externalBoundObject.listaSelectoare.FirstOrDefault(x => x.ID == "categorie").Selector;

            string script = $"fetch('https://fermier.ro')"
             + ".then(response => response.text())"
             + ".then(html => new DOMParser().parseFromString(html, 'text/html'))"
             + ".then(doc => {"
             + $" return  Array.from(doc.querySelectorAll('{selector_categorie}')).map(link => ({{CategoryName: link.textContent.trim(), CategoryLink: link.href || null }}))"
             + "});";

            var result = await chromeBrowser.EvaluateScriptAsync(script);

            var productInformation = result.Success ? result.Result : "N/A"; 

            if (result.Result is List<object> categoryList)
            {
                foreach (var categoryObj in categoryList)
                {
                    
                        dynamic dynamicCategory = categoryObj;
                        string categoryName = dynamicCategory.CategoryName;
                        string categoryLink = dynamicCategory.CategoryLink;


                    if (!categories.Any(c => c.CategoryName == categoryName && c.CategoryLink == categoryLink))
                    {
                        categories.Add(new Category
                        {
                            CategoryName = categoryName,
                            CategoryLink = categoryLink
                        });
                    }

                }

                int i = 0;
                foreach (var category in categories)
                {
                    Console.WriteLine(category+"----------------------------------------------------");
                  // if(i==3)
                  //  {
                      await ScrapeCategoryAndSubcategories(category);
                  // }
                   // i++;
                }
                MessageBox.Show("Scrapping-ul a fost finalizat.", "Scrapping Terminat", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
            {
               
                Console.WriteLine($"Script execution failed: {result.Message}");
            }

        }


        private async Task ScrapeCategoryAndSubcategories(Category category, string parentCategory = "")
        {
            string selector_subcategorie = externalBoundObject.listaSelectoare.FirstOrDefault(x => x.ID == "subcategorie")?.Selector;

            string script = $"fetch('{category.CategoryLink}')"
             + ".then(response => response.text())"
             + ".then(html => new DOMParser().parseFromString(html, 'text/html'))"
             + ".then(doc => {"
             + $" return  Array.from(doc.querySelectorAll('{selector_subcategorie}')).map(link => ({{CategoryName: link.textContent.trim(), CategoryLink: link.href || null }}))"
             + "});";
            var result = await chromeBrowser.EvaluateScriptAsync(script);

            List<object> subCategoryList = result.Result as List<object>;

            if (subCategoryList != null && subCategoryList.Any())
            {
                foreach (var subCategoryObj in subCategoryList)
                {
                    if (subCategoryObj != null)
                    {
                        dynamic dynamicSubCategory = subCategoryObj;

                        string categoryNameWithoutProducts = dynamicSubCategory.CategoryName;
                        Match match = Regex.Match(categoryNameWithoutProducts, @"\d+ produse");
                        if (match.Success)
                        {
                            categoryNameWithoutProducts = categoryNameWithoutProducts.Replace(match.Value, "").Trim();
                        }
                        Category subCategory = new Category
                        {
                            CategoryName = categoryNameWithoutProducts,
                            CategoryLink = dynamicSubCategory.CategoryLink,
                        };

                        // Recursivitate pentru subcategorii
                        await ScrapeCategoryAndSubcategories(subCategory);

                        if (!category.Subcategories.Any(existingSubCategory => existingSubCategory.CategoryLink == subCategory.CategoryLink))
                        {
                            // Adăugăm subcategoria la lista de subcategorii a categoriei părinte doar dacă nu există deja
                            category.Subcategories.Add(subCategory);
                        }
                    }
                }
            }
            else
            {
                // Nu există subcategorii sau lista de subcategorii este goală, reia produsele de pe pagina principală
                Console.WriteLine($"-------------{category.CategoryName}-------------");
                var rez = await ScrapePages(category.CategoryLink, parentCategory, category.CategoryName);
                category.Products = rez;
                produse.AddRange(rez);
            }
        }


        private void button3_Click_2(object sender, EventArgs e)
        {
            // Inițializează un nou dialog de salvare
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            // Setează titlul dialogului
            saveFileDialog1.Title = "Salvare fișier";

            // Setează filtrul de fișiere (opțional, pentru a limita tipurile de fișiere care pot fi salvate)
            saveFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            // Deschide dialogul de salvare și verifică dacă utilizatorul a selectat o locație și a confirmat salvarea
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Obține calea fișierului selectat de utilizator din dialogul de salvare
                string filePath = saveFileDialog1.FileName;

                // Apelul metodei pentru salvarea datelor în fișierul selectat
                externalBoundObject.SaveDataToFile(filePath);

                // Opțional, puteți afișa un mesaj de confirmare că salvarea a fost realizată cu succes
                MessageBox.Show("Datele au fost salvate cu succes.", "Salvare reușită", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Inițializează un nou dialog de deschidere a fișierului
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Setează titlul dialogului
            openFileDialog1.Title = "Selectare fișier";

            // Setează filtrul de fișiere (opțional, pentru a limita tipurile de fișiere care pot fi selectate)
            openFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            // Deschide dialogul de deschidere a fișierului și verifică dacă utilizatorul a selectat un fișier și a confirmat deschiderea
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Obține calea fișierului selectat de utilizator din dialogul de deschidere a fișierului
                string filePath = openFileDialog1.FileName;

                // Apelul metodei pentru încărcarea și afișarea datelor din fișierul selectat
                externalBoundObject.LoadAndDisplayDataFromFile(filePath);

                // Opțional, puteți afișa un mesaj de confirmare că datele au fost încărcate cu succes
                MessageBox.Show("Datele au fost încărcate și afișate cu succes.", "Încărcare și afișare reușită", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void comboType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

}
