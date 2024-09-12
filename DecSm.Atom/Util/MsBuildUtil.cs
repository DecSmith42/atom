namespace DecSm.Atom.Util;

public static class MsBuildUtil
{
    public static string SetVersionInfo(string file, SemVer version)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(file);

        // First, remove all existing version properties
        // This is to ensure that we don't have duplicate properties
        var propertyGroups = xmlDocument.SelectNodes("//PropertyGroup");

        if (propertyGroups is not null)
            foreach (XmlNode propertyGroup in propertyGroups)
            foreach (XmlNode property in propertyGroup.ChildNodes)
                if (property is XmlElement
                    {
                        Name: "Version"
                        or "VersionPrefix"
                        or "VersionSuffix"
                        or "PackageVersion"
                        or "AssemblyVersion"
                        or "FileVersion"
                        or "InformationalVersion",
                    })
                    propertyGroup.RemoveChild(property);

        // Create a new property group and populate it with the version properties
        var propertyGroupNode = xmlDocument.CreateElement("PropertyGroup", xmlDocument.DocumentElement?.NamespaceURI);

        AddProperty("Version", version.Prefix, xmlDocument, propertyGroupNode);
        AddProperty("PackageVersion", version, xmlDocument, propertyGroupNode);
        AddProperty("AssemblyVersion", $"{version.Prefix}.0", xmlDocument, propertyGroupNode);
        AddProperty("FileVersion", $"{version.Prefix}.{version.BuildNumberFromPreRelease}", xmlDocument, propertyGroupNode);
        AddProperty("InformationalVersion", version, xmlDocument, propertyGroupNode);

        xmlDocument.DocumentElement?.AppendChild(propertyGroupNode);

        return xmlDocument.OuterXml;
    }

    private static void AddProperty(string name, string value, XmlDocument xmlDocument, XmlElement propertyGroupNode)
    {
        var propertyNode = xmlDocument.CreateElement(name, xmlDocument.DocumentElement?.NamespaceURI);
        propertyNode.InnerText = value;
        propertyGroupNode.AppendChild(propertyNode);
    }
}
