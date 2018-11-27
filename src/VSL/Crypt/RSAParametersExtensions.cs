using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace VSL.Crypt
{
    /// <summary>
    /// Provides extension methods for easier usage of <see cref="RSAParameters"/>.
    /// </summary>
    public static class RSAParametersExtensions
    {
        /// <summary>
        /// Imports an XML formatted key in this <see cref="RSAParameters"/> struct.
        /// </summary>
        public static RSAParameters ImportXmlKey(this RSAParameters parameters, string xmlKey)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlKey)))
            {
                string open = "";
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                        open = reader.Name;
                    if (reader.NodeType == XmlNodeType.Text)
                    {
                        string val = reader.Value;
                        byte[] valB = Convert.FromBase64String(val);
                        switch (open)
                        {
                            case "Modulus":
                                parameters.Modulus = valB;
                                break;
                            case "Exponent":
                                parameters.Exponent = valB;
                                break;
                            case "P":
                                parameters.P = valB;
                                break;
                            case "Q":
                                parameters.Q = valB;
                                break;
                            case "DP":
                                parameters.DP = valB;
                                break;
                            case "DQ":
                                parameters.DQ = valB;
                                break;
                            case "InverseQ":
                                parameters.InverseQ = valB;
                                break;
                            case "D":
                                parameters.D = valB;
                                break;
                        }
                    }
                }
            }

            return parameters;
        }

        /// <summary>
        /// Exports this <see cref="RSAParameters"/> struct to an XML formatted key.
        /// </summary>
        public static string ExportXmlKey(this RSAParameters parameters)
        {
            StringBuilder result = new StringBuilder();
            var settings = new XmlWriterSettings() { ConformanceLevel = ConformanceLevel.Fragment, OmitXmlDeclaration = true };
            using (XmlWriter writer = XmlWriter.Create(result, settings))
            {
                writer.WriteStartElement("RSAKeyValue");
                if (ValidByteArray(parameters.Modulus))
                    writer.WriteElementString("Modulus", Convert.ToBase64String(parameters.Modulus));
                if (ValidByteArray(parameters.Exponent))
                    writer.WriteElementString("Exponent", Convert.ToBase64String(parameters.Exponent));
                if (ValidByteArray(parameters.P))
                    writer.WriteElementString("P", Convert.ToBase64String(parameters.P));
                if (ValidByteArray(parameters.Q))
                    writer.WriteElementString("Q", Convert.ToBase64String(parameters.Q));
                if (ValidByteArray(parameters.DP))
                    writer.WriteElementString("DP", Convert.ToBase64String(parameters.DP));
                if (ValidByteArray(parameters.DQ))
                    writer.WriteElementString("DQ", Convert.ToBase64String(parameters.DQ));
                if (ValidByteArray(parameters.InverseQ))
                    writer.WriteElementString("InverseQ", Convert.ToBase64String(parameters.InverseQ));
                if (ValidByteArray(parameters.D))
                    writer.WriteElementString("D", Convert.ToBase64String(parameters.D));
                writer.WriteEndElement();
            }

            return result.ToString();
        }

        internal static void AssertValid(this RSAParameters parameters)
        {
            if (parameters.Modulus == null ||
                parameters.Exponent == null)
                throw new ArgumentException("The RSAParameters struct is invalid.");
        }

        private static bool ValidByteArray(byte[] buffer)
        {
            return buffer != null && buffer.Length > 0;
        }
    }
}
