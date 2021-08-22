using Spire.Pdf;
using Spire.Pdf.Actions;
using Spire.Pdf.AutomaticFields;
using Spire.Pdf.Fields;
using Spire.Pdf.Graphics;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.XPath;

namespace PDF_Generator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            var GenerateHelloWorldButton = new Button
            {
                Height = 40,
                Dock = DockStyle.Top,
                Text = "PDF HelloWorld (HelloWorld.pdf)"
            };
            var GenerateFormButton = new Button
            {
                Height = 40,
                Dock = DockStyle.Top,
                Text = "PDF Форма из XML (FormField.pdf)"
            };

            GenerateFormButton.Click += GeneratePDF;
            GenerateHelloWorldButton.Click += GenerateHelloWorldPDF;
            Controls.Add(GenerateFormButton);
            Controls.Add(GenerateHelloWorldButton);


        }

        private void GenerateHelloWorldPDF(object sender, EventArgs e)
        {
            //Create a pdf document
            PdfDocument doc = new PdfDocument();

            //Create one page
            PdfPageBase page = doc.Pages.Add();

            //Draw the text
            page.Canvas.DrawString("Hello, World!",
                                   new PdfFont(PdfFontFamily.TimesRoman, 30f),
                                   new PdfSolidBrush(Color.Black),
                                   10, 10);

            String result = "HelloWorld.pdf";

            //Save the document
            doc.SaveToFile(result);
            doc.Close();

        }


        private void GeneratePDF(object sender, EventArgs e)
        {
            //Create a pdf document.
            PdfDocument doc = new PdfDocument();

            //margin
            PdfUnitConvertor unitCvtr = new PdfUnitConvertor();
            PdfMargins margin = new PdfMargins();
            margin.Top = unitCvtr.ConvertUnits(2.54f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            margin.Bottom = margin.Top;
            margin.Left = unitCvtr.ConvertUnits(3.17f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            margin.Right = margin.Left;

            SetDocumentTemplate(doc, PdfPageSize.A4, margin);

            PdfPageBase page = doc.Pages.Add(PdfPageSize.A4, new PdfMargins(0));
            float y = 20;
            y = DrawPageTitle(page, y);

            using (Stream stream = File.OpenRead(@"Form.xml"))
            {
                XPathDocument xpathDoc = new XPathDocument(stream);
                XPathNodeIterator sectionNodes = xpathDoc.CreateNavigator().Select("/form/section");

                int fieldIndex = 0;
                foreach (XPathNavigator sectionNode in sectionNodes)
                {
                    //Draw Section Label
                    String sectionLabel = sectionNode.GetAttribute("name", "");
                    y = DrawFormSection(sectionLabel, page, y);

                    XPathNodeIterator fieldNodes = sectionNode.Select("field");
                    foreach (XPathNavigator fieldNode in fieldNodes)
                    {
                        y = DrawFormField(fieldNode, doc.Form, page, y, fieldIndex++);
                    }
                }
            }

            y += 20;
            float buttonWidth = 80;
            float buttonX = (page.Canvas.ClientSize.Width - buttonWidth) / 2;
            RectangleF buttonBounds = new RectangleF(buttonX, y, buttonWidth, 16f);
            PdfButtonField button = new PdfButtonField(page, "submit");
            button.Text = "Submit";
            button.Bounds = buttonBounds;
            button.BorderColor = Color.DarkCyan;
            button.BackColor = Color.GhostWhite;
            PdfSubmitAction submitAction = new PdfSubmitAction("http://ya.ru");
            button.Actions.MouseUp = submitAction;
            doc.Form.Fields.Add(button);

            doc.SaveToFile("FormField.pdf");
            doc.Close();

        }

        static float DrawFormSection(String label, PdfPageBase page, float y)
        {
            PdfBrush brush1 = PdfBrushes.GhostWhite;
            PdfBrush brush2 = PdfBrushes.DeepSkyBlue;
            PdfTrueTypeFont font = new PdfTrueTypeFont(new Font("Calibri", 11f, FontStyle.Bold));
            float height = font.MeasureString(label).Height;
            page.Canvas.DrawRectangle(brush2, 0, y, page.Canvas.ClientSize.Width, height + 2);
            page.Canvas.DrawString(label, font, brush1, 2, y + 1);
            y = y + height + 2;
            PdfPen pen = new PdfPen(PdfBrushes.DeepSkyBlue, 0.25f);
            page.Canvas.DrawLine(pen, 0, y, page.Canvas.ClientSize.Width, y);
            return y + 0.75f;
        }

        static float DrawFormField(XPathNavigator fieldNode, PdfForm form, PdfPageBase page, float y, int fieldIndex)
        {
            float width = page.Canvas.ClientSize.Width;
            float padding = 2;
            String label = fieldNode.GetAttribute("label", "");
            PdfTrueTypeFont font1 = new PdfTrueTypeFont(new Font("Calibri", 10f));
            PdfStringFormat format = new PdfStringFormat(PdfTextAlignment.Right, PdfVerticalAlignment.Middle);
            float labelMaxWidth = width * 0.4f - 2 * padding;
            SizeF labelSize = font1.MeasureString(label, labelMaxWidth, format);

            float fieldHeight = MeasureFieldHeight(fieldNode);
            float height = labelSize.Height > fieldHeight ? labelSize.Height : fieldHeight;
            height += 2;

            PdfBrush brush = PdfBrushes.GhostWhite;
            page.Canvas.DrawRectangle(brush, 0, y, width, height);
            PdfBrush brush1 = PdfBrushes.DarkCyan;
            RectangleF labelBounds = new RectangleF(padding, y, labelMaxWidth, height);
            page.Canvas.DrawString(label, font1, brush1, labelBounds, format);



            float fieldMaxWidth = width * 0.57f - 2 * padding;
            float fieldX = labelBounds.Right + 2 * padding;
            float fieldY = y + (height - fieldHeight) / 2;
            String fieldType = fieldNode.GetAttribute("type", "");
            String fieldId = fieldNode.GetAttribute("id", "");
            bool required = "true" == fieldNode.GetAttribute("required", "");
            switch (fieldType)
            {
                case "text":
                case "password":
                    PdfTextBoxField textField = new PdfTextBoxField(page, fieldId);
                    textField.Bounds = new RectangleF(fieldX, fieldY, fieldMaxWidth, fieldHeight);
                    textField.BorderWidth = 0.25f;
                    textField.BorderStyle = PdfBorderStyle.Solid;
                    textField.Required = required;
                    if ("password" == fieldType)
                    {
                        textField.Password = true;
                    }
                    if ("true" == fieldNode.GetAttribute("multiple", ""))
                    {
                        textField.Multiline = true;
                        textField.Scrollable = true;
                    }
                    form.Fields.Add(textField);
                    break;
                case "checkbox":
                    PdfCheckBoxField checkboxField = new PdfCheckBoxField(page, fieldId);
                    float checkboxWidth = fieldHeight - 2 * padding;
                    float checkboxHeight = checkboxWidth;
                    checkboxField.Bounds = new RectangleF(fieldX, fieldY + padding, checkboxWidth, checkboxHeight);
                    checkboxField.BorderWidth = 0.25f;
                    checkboxField.Style = PdfCheckBoxStyle.Cross;
                    checkboxField.Required = required;
                    form.Fields.Add(checkboxField);
                    break;

                case "list":
                    XPathNodeIterator itemNodes = fieldNode.Select("item");
                    if ("true" == fieldNode.GetAttribute("multiple", ""))
                    {
                        PdfListBoxField listBoxField = new PdfListBoxField(page, fieldId);
                        listBoxField.Bounds = new RectangleF(fieldX, fieldY, fieldMaxWidth, fieldHeight);
                        listBoxField.BorderWidth = 0.25f;
                        listBoxField.MultiSelect = true;
                        listBoxField.Font = new PdfFont(PdfFontFamily.Helvetica, 9f);
                        listBoxField.Required = required;
                        //add items into list box.
                        foreach (XPathNavigator itemNode in itemNodes)
                        {
                            String text = itemNode.SelectSingleNode("text()").Value;
                            listBoxField.Items.Add(new PdfListFieldItem(text, text));
                        }
                        listBoxField.SelectedIndex = 0;
                        form.Fields.Add(listBoxField);

                        break;
                    }
                    if (itemNodes != null && itemNodes.Count <= 3)
                    {
                        PdfRadioButtonListField radioButtonListFile
                            = new PdfRadioButtonListField(page, fieldId);
                        radioButtonListFile.Required = required;
                        //add items into radio button list.
                        float fieldItemHeight = fieldHeight / itemNodes.Count;
                        float radioButtonWidth = fieldItemHeight - 2 * padding;
                        float radioButtonHeight = radioButtonWidth;
                        foreach (XPathNavigator itemNode in itemNodes)
                        {
                            String text = itemNode.SelectSingleNode("text()").Value;
                            PdfRadioButtonListItem fieldItem = new PdfRadioButtonListItem(text);
                            fieldItem.BorderWidth = 0.25f;
                            fieldItem.Bounds = new RectangleF(fieldX, fieldY + padding, radioButtonWidth, radioButtonHeight);
                            radioButtonListFile.Items.Add(fieldItem);

                            float fieldItemLabelX = fieldX + radioButtonWidth + padding;
                            SizeF fieldItemLabelSize = font1.MeasureString(text);
                            float fieldItemLabelY = fieldY + (fieldItemHeight - fieldItemLabelSize.Height) / 2;
                            page.Canvas.DrawString(text, font1, brush1, fieldItemLabelX, fieldItemLabelY);

                            fieldY = fieldY + fieldItemHeight;
                        }
                        form.Fields.Add(radioButtonListFile);

                        break;
                    }

                    //combo box
                    PdfComboBoxField comboBoxField = new PdfComboBoxField(page, fieldId);
                    comboBoxField.Bounds = new RectangleF(fieldX, fieldY, fieldMaxWidth, fieldHeight);
                    comboBoxField.BorderWidth = 0.25f;
                    comboBoxField.Font = new PdfFont(PdfFontFamily.Helvetica, 9f);
                    comboBoxField.Required = required;
                    //add items into combo box.
                    foreach (XPathNavigator itemNode in itemNodes)
                    {
                        String text = itemNode.SelectSingleNode("text()").Value;
                        comboBoxField.Items.Add(new PdfListFieldItem(text, text));
                    }
                    form.Fields.Add(comboBoxField);
                    break;
            }

            if (required)
            {
                //draw *
                float flagX = width * 0.97f + padding;
                PdfTrueTypeFont font3 = new PdfTrueTypeFont(new Font("Calibri", 10f, FontStyle.Bold));
                SizeF size = font3.MeasureString("*");
                float flagY = y + (height - size.Height) / 2;
                page.Canvas.DrawString("*", font3, PdfBrushes.Red, flagX, flagY);
            }
            return y + fieldHeight + 0.75f;

        }

        static float MeasureFieldHeight(XPathNavigator fieldNode)
        {
            String fieldType = fieldNode.GetAttribute("type", "");
            float defaultHeight = 16f;
            switch (fieldType)
            {
                case "text":
                case "password":
                    if ("true" == fieldNode.GetAttribute("multiple", ""))
                    {
                        return defaultHeight * 3;
                    }
                    return defaultHeight;

                case "checkbox":
                    return defaultHeight;

                case "list":
                    if ("true" == fieldNode.GetAttribute("multiple", ""))
                    {
                        return defaultHeight * 3;
                    }
                    XPathNodeIterator itemNodes = fieldNode.Select("item");
                    if (itemNodes != null && itemNodes.Count <= 3)
                    {
                        return defaultHeight * 3;
                    }
                    return defaultHeight;
            }
            String message = String.Format("Invalid field type: {0}", fieldType);
            throw new ArgumentException(message);
        }

        private float DrawPageTitle(PdfPageBase page, float y)
        {
            PdfBrush brush1 = PdfBrushes.DarkCyan;
            PdfBrush brush2 = PdfBrushes.Red;
            PdfTrueTypeFont font1 = new PdfTrueTypeFont(new Font("Calibri", 12f, FontStyle.Bold));
            String title = "Your Account Information(* Required)";
            SizeF size = font1.MeasureString(title);
            float x = (page.Canvas.ClientSize.Width - size.Width) / 2;
            page.Canvas.DrawString("Your Account Information(", font1, brush1, x, y);
            size = font1.MeasureString("Your Account Information(");
            x = x + size.Width;
            page.Canvas.DrawString("* Required", font1, brush2, x, y);
            size = font1.MeasureString("* Required");
            x = x + size.Width;
            page.Canvas.DrawString(")", font1, brush1, x, y);
            y = y + size.Height;

            y = y + 3;
            PdfTrueTypeFont font2 = new PdfTrueTypeFont(new Font("Calibri", 9f, FontStyle.Italic));
            String p = "Your information is protected and will not be shared with anyone.";
            page.Canvas.DrawString(p, font2, PdfBrushes.Black, 0, y);
            return y + font2.Height;
        }

        private void SetDocumentTemplate(PdfDocument doc, SizeF pageSize, PdfMargins margin)
        {
            PdfPageTemplateElement leftSpace
                = new PdfPageTemplateElement(margin.Left, pageSize.Height);
            doc.Template.Left = leftSpace;

            PdfPageTemplateElement topSpace
                = new PdfPageTemplateElement(pageSize.Width, margin.Top);
            topSpace.Foreground = true;
            doc.Template.Top = topSpace;

            PdfTrueTypeFont font = new PdfTrueTypeFont(new Font("Calibri", 10f, FontStyle.Regular));
            PdfStringFormat format = new PdfStringFormat(PdfTextAlignment.Right);
            String label = "Demo of Spire.Pdf";
            SizeF size = font.MeasureString(label, format);
            float y = topSpace.Height - font.Height - 1;
            PdfPen pen = new PdfPen(Color.SlateGray, 0.75f);
            topSpace.Graphics.SetTransparency(0.5f);
            topSpace.Graphics.DrawLine(pen, margin.Left, y, pageSize.Width - margin.Right, y);
            y = y - 1 - size.Height;
            topSpace.Graphics.DrawString(label, font, PdfBrushes.SlateGray, pageSize.Width - margin.Right, y, format);

            PdfPageTemplateElement rightSpace
               = new PdfPageTemplateElement(margin.Right, pageSize.Height);
            doc.Template.Right = rightSpace;

            PdfPageTemplateElement bottomSpace
                = new PdfPageTemplateElement(pageSize.Width, margin.Bottom);
            bottomSpace.Foreground = true;
            doc.Template.Bottom = bottomSpace;

            y = font.Height + 1;
            bottomSpace.Graphics.SetTransparency(0.5f);
            bottomSpace.Graphics.DrawLine(pen, margin.Left, y, pageSize.Width - margin.Right, y);
            y++;
            PdfPageNumberField pageNumber = new PdfPageNumberField();
            PdfPageCountField pageCount = new PdfPageCountField();
            PdfCompositeField pageNumberLabel = new PdfCompositeField();
            pageNumberLabel.AutomaticFields
                = new PdfAutomaticField[] { pageNumber, pageCount };
            pageNumberLabel.Brush = PdfBrushes.SlateGray;
            pageNumberLabel.Font = font;
            pageNumberLabel.StringFormat = format;
            pageNumberLabel.Text = "page {0} of {1}";
            pageNumberLabel.Draw(bottomSpace.Graphics, pageSize.Width - margin.Right, y);


        }
    }
}
