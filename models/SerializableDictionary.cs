using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using OfficeOpenXml.Drawing.Chart.ChartEx;
using System.Collections;

namespace WebScrapingEcap.models
{
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException("Metoda ReadXml nu este implementată.");
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (KeyValuePair<TKey, TValue> kvp in this)
            {              
                writer.WriteStartElement(kvp.Key.ToString());
                
                // Verificăm dacă valoarea este o listă
                if (kvp.Value is IList listValue)
                {
                    // Dacă este o listă, serializăm fiecare element al listei separat
                    foreach (var item in listValue)
                    {
                        
                        writer.WriteString(item.ToString());

                    }
                }
                else
                {
                    if (kvp.Value is IDictionary innerDictionary)
                    {
                        // Dacă valoarea este un dicționar, o serializăm recursiv
                        SerializeDictionary(writer, innerDictionary);
                    }
                    else
                    {
                        // Dacă nu este un dicționar, o serializăm direct
                        if (kvp.Value != null)
                        {
                            writer.WriteString(kvp.Value.ToString());
                        }
                    }
                }

                                
                writer.WriteEndElement();


            }
        }

        private void SerializeDictionary(XmlWriter writer, IDictionary dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];

                // Verificăm dacă valoarea este un dicționar
                if (value is IDictionary innerDictionary)
                {
                    // Dacă este un dicționar, serializăm recursiv fiecare pereche cheie-valoare
                    writer.WriteStartElement(key.ToString()); // Folosim cheia ca etichetă
                    SerializeDictionary(writer, innerDictionary);
                    writer.WriteEndElement(); // </Key>
                }
                else if (value is IList listValue)
                {
                    // Verificăm dacă valoarea este o listă
                    foreach (var item in listValue)
                    {
                        writer.WriteStartElement(key.ToString()); // Folosim cheia ca etichetă pentru fiecare element din listă
                        writer.WriteValue(item);
                        writer.WriteEndElement(); // </Key>
                    }
                }
                else
                {
                    // Dacă nu este un dicționar sau o listă, o serializăm direct
                    writer.WriteStartElement(key.ToString());
                    writer.WriteValue(value);
                    writer.WriteEndElement(); // </Key>
                }
            }
        }



    }

}
