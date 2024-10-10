using CefSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebScrapingEcap.models
{
   
    public class ExternalBoundObject
    {
        public readonly Chromium form;
        public string verifica = "nimic";
        private bool isBtnSelectActive;
        private bool isMultipleCheckActive;
        public event EventHandler<bool> SelectBtnStateChanged;
        public event EventHandler<bool> MultipleCheckChanged;
        public List<SelectorInfo> listaSelectoare = new List<SelectorInfo>(); 



        public ExternalBoundObject(Chromium form)
        {
            this.form = form;
            form.dataGridView1.CellContentClick += DataGridView1_CellContentClick;
        }

        public string UpdateComboBox(string selector)
        {
            return selector;
           
        }

        public void SetCheckMultiple(bool check)
        {
            isMultipleCheckActive = check;
            OnMultiplecheckStateChanged(isMultipleCheckActive);
        }


        public bool GetCheckMultiple()
        {
            return isMultipleCheckActive;
        }


        private void OnMultiplecheckStateChanged(bool newState)
        {
            MultipleCheckChanged?.Invoke(this, newState);
        }

        public void SetSelectBtn(bool select)
        {
            isBtnSelectActive = select;
            OnSelectBtnStateChanged(isBtnSelectActive);
        }

        public bool GetSelectBtn()
        {
            return isBtnSelectActive;
        }

        private void OnSelectBtnStateChanged(bool newState)
        {
            SelectBtnStateChanged?.Invoke(this, newState);
        }

        public void AddSelectorToList(SelectorInfo selector)
        {
            if (!string.IsNullOrEmpty(selector.Selector))
            {
                listaSelectoare.Add(selector);
                // Actualizează controlul GUI (ListBox, DataGridView, etc.) pentru a reflecta schimbările
                this.UpdateDataGridView();
            }
        }

        public void DeleteRow(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < listaSelectoare.Count)
            {
                listaSelectoare.RemoveAt(rowIndex);
                UpdateDataGridView();
            }
        }


        public void UpdateDataGridView()
        {
            if (form != null && form.dataGridView1 != null)
            {
                if (form.dataGridView1.InvokeRequired)
                {
                    form.dataGridView1.Invoke(new Action(() =>
                    {
                        form.dataGridView1.Rows.Clear();
                        form.dataGridView1.Columns.Clear();

                        form.dataGridView1.Columns.Add("ID", "ID");
                        form.dataGridView1.Columns.Add("Selector", "Selector");
                        form.dataGridView1.Columns.Add("Type", "Type");
                        form.dataGridView1.Columns.Add("IsList", "IsList");

                        DataGridViewButtonColumn deleteButtonColumn = new DataGridViewButtonColumn();
                        deleteButtonColumn.HeaderText = "Delete";
                        deleteButtonColumn.Text = "Delete";
                        deleteButtonColumn.UseColumnTextForButtonValue = true;
                        form.dataGridView1.Columns.Add(deleteButtonColumn);

                        foreach (SelectorInfo selector in listaSelectoare)
                        {
                            form.dataGridView1.Rows.Add(selector.ID, selector.Selector, selector.Type, selector.IsList);
                        }
                    }));
                }
                else
                {
                    form.dataGridView1.Rows.Clear();
                    form.dataGridView1.Columns.Clear();

                    form.dataGridView1.Columns.Add("ID", "ID");
                    form.dataGridView1.Columns.Add("Selector", "Selector");
                    form.dataGridView1.Columns.Add("Type", "Type");
                    form.dataGridView1.Columns.Add("IsList", "IsList");

                    DataGridViewButtonColumn deleteButtonColumn = new DataGridViewButtonColumn();
                    deleteButtonColumn.HeaderText = "Delete";
                    deleteButtonColumn.Text = "Delete";
                    deleteButtonColumn.UseColumnTextForButtonValue = true;
                    form.dataGridView1.Columns.Add(deleteButtonColumn);

                    foreach (SelectorInfo selector in listaSelectoare)
                    {
                        form.dataGridView1.Rows.Add(selector.ID, selector.Selector, selector.Type, selector.IsList);
                    }
                }
            }
        }


        public void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewCell cell = form.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];

                // Verificăm dacă tipul celulei este de tipul DataGridViewButtonCell
                if (cell is DataGridViewButtonCell)
                {
                    // Aici puteți adăuga orice alte verificări sau acțiuni necesare pentru butonul specific apăsat

                    // Apelăm metoda pentru a șterge rândul din listaSelectoare
                    DeleteRow(e.RowIndex);
                }
            }
        }








        public void UpdateTextBox(string selector)
        {
            
            if (form != null && form.textBox1 != null)
            {
                // Verificăm dacă trebuie să invocăm acțiunea pe thread-ul UI
                if (form.textBox1.InvokeRequired)
                {
                    // Folosim Invoke pentru a executa acțiunea pe thread-ul UI
                    form.textBox1.Invoke(new Action(() => form.textBox1.Text = selector));
                }
                else
                {
                    // Dacă suntem deja pe thread-ul UI, putem actualiza direct TextBox-ul
                    form.textBox1.Text = selector;
                }
            }
        }

        public void SaveDataToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (SelectorInfo selector in listaSelectoare)
                {
                    writer.WriteLine($"{selector.ID},{selector.Selector},{selector.Type},{selector.IsList}");
                }
            }
        }

        public List<SelectorInfo> LoadDataFromFile(string filePath)
        {
            List<SelectorInfo> loadedData = new List<SelectorInfo>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 4)
                    {
                        SelectorInfo selector = new SelectorInfo
                        {
                            ID = parts[0],
                            Selector = parts[1],
                            Type = (SelectorType)Enum.Parse(typeof(SelectorType), parts[2]),
                            IsList = bool.Parse(parts[3])
                        };
                        loadedData.Add(selector);
                    }
                }
            }

            return loadedData;
        }


        public void LoadAndDisplayDataFromFile(string filePath)
        {
            List<SelectorInfo> loadedData = LoadDataFromFile(filePath);
            listaSelectoare.Clear();
            listaSelectoare.AddRange(loadedData);
            UpdateDataGridView();
        }




    }

}
