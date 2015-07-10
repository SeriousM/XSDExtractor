#region License
/*
JFDI the .Net Job Framework (http://jfdi.sourceforge.net)
Copyright (C) 2006  Steven Ward (steve.ward.uk@gmail.com)

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA
*/
#endregion

using System.Configuration;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace JFDI.Utils.XSDExtractor.Parsers {
  
  /// <summary>
  /// Responsible for converting a ConfigurationElement into
  /// XmlSchema objects
  /// </summary>
  public class ConfigurationElementParser : TypeParser {
  
    /// <summary>
    /// 
    /// </summary>
    public ConfigurationElementParser(XSDGenerator generator)
      : base(generator) {
    }

    /// <summary>
    /// 
    /// </summary>
    public override void GenerateSchemaTypeObjects(PropertyInfo property, XmlSchemaType type) {

      ConfigurationPropertyAttribute[] atts = GetAttributes<ConfigurationPropertyAttribute>(property);
      if (atts.Length == 0)
        return;

      XmlSchemaComplexType ct;
      if (generator.ComplexMap.ContainsKey(property.PropertyType)) { 
        
        //already done the work
        ct = generator.ComplexMap[property.PropertyType]; 

      } else {
        
        //  got to generate a new one
        ct = new XmlSchemaComplexType();
        ct.Name = atts[0].Name + "CT";
        XMLHelper.CreateSchemaSequenceParticle(ct);

        generator.ComplexMap.Add(property.PropertyType, ct);
        generator.Schema.Items.Add(ct);

      }

      XmlSchemaElement element = new XmlSchemaElement();
      element.Name = atts[0].Name;
      element.MinOccurs = atts[0].IsRequired ? 1 : 0;
      element.SchemaTypeName = new XmlQualifiedName(XMLHelper.PrependNamespaceAlias(ct.Name));
      XmlSchemaComplexType pct = type as XmlSchemaComplexType;
      ((XmlSchemaGroupBase)pct.Particle).Items.Add(element);

      //  add the documentation
      AddAnnotation(property, element, atts[0]);

      //  get all properties from the configuration object
      foreach (PropertyInfo pi in GetProperties<ConfigurationPropertyAttribute>(property.PropertyType)) {

        TypeParser parser = TypeParserFactory.GetParser(generator, pi);
        parser.GenerateSchemaTypeObjects(pi, ct);

      }

    }

  }

}