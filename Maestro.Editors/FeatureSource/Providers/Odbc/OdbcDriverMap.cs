#region Disclaimer / License

// Copyright (C) 2010, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

#endregion Disclaimer / License

using System;
using System.Collections.Generic;
using System.Xml;

namespace Maestro.Editors.FeatureSource.Providers.Odbc
{
    internal static class OdbcDriverMap
    {
        private static readonly Dictionary<string, Type> _drivers;

        static OdbcDriverMap()
        {
            _drivers = new Dictionary<string, Type>();
            if (System.IO.File.Exists("OdbcEditorMap.xml")) //NOXLATE
            {
                var doc = new XmlDocument();
                doc.Load("OdbcEditorMap.xml"); //NOXLATE
                var list = doc.SelectNodes("//OdbcDriverMap/Driver"); //NOXLATE
                foreach (XmlNode node in list)
                {
                    try
                    {
                        string provider = node.Attributes["name"].Value.ToUpper(); //NOXLATE
                        string typeName = node.Attributes["type"].Value; //NOXLATE

                        _drivers[provider] = Type.GetType(typeName);
                    }
                    catch { }
                }
            }
        }

        public static string[] EnumerateDrivers()
        {
            return new List<string>(_drivers.Keys).ToArray();
        }

        public static OdbcDriverInfo GetDriver(string provider)
        {
            OdbcDriverInfo driver = null;

            string name = provider.ToUpper();
            if (_drivers.ContainsKey(name))
            {
                driver = (OdbcDriverInfo)Activator.CreateInstance(_drivers[name]);
            }
            else
            {
                throw new OdbcDriverNotFoundException(provider);
            }

            return driver;
        }
    }

    /// <summary>
    /// Thrown when an ODBC driver is not found
    /// </summary>
    public class OdbcDriverNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OdbcDriverNotFoundException"/> class.
        /// </summary>
        public OdbcDriverNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdbcDriverNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public OdbcDriverNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdbcDriverNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public OdbcDriverNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}