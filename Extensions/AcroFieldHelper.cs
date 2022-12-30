namespace Simplz.PDFForm.DataExtractor.Extensions
{
    using UglyToad.PdfPig.AcroForms.Fields;
    using UglyToad.PdfPig.AcroForms;

    public static class AcroFieldHelper
    {
        public static IEnumerable<AcroFieldBase> GetFields(this AcroForm form)
        {
            return form.Fields.GetFields();
        }

        public static IEnumerable<AcroFieldBase> GetFields(this IEnumerable<AcroFieldBase> fieldBases)
        {
            foreach (var fieldBase in fieldBases)
            {
                if (fieldBase.FieldType != AcroFieldType.Unknown)
                    yield return fieldBase;
                if (fieldBase is AcroNonTerminalField nonTerminalField)
                    foreach (var child in nonTerminalField.Children.GetFields())
                        yield return child;
            }
        }

        public static IEnumerable<AcroFieldBase> GetFields(this AcroFieldBase fieldBase)
        {
            if (fieldBase.FieldType != AcroFieldType.Unknown)
                yield return fieldBase;
            if (fieldBase is AcroNonTerminalField nonTerminalField)
                foreach (var child in nonTerminalField.Children)
                    foreach (var item in child.GetFields())
                        yield return item;
        }

        public static KeyValuePair<string, string?> GetFieldValue(this AcroFieldBase fieldBase)
        {
            return fieldBase switch
            {
                AcroTextField textField => new(textField.Information.PartialName, textField.Value),
                AcroCheckboxField checkboxField => new(checkboxField.Information.PartialName, checkboxField.IsChecked.ToString()),
                _ => new(fieldBase.Information.PartialName, ""),
            };
        }
    }
}