# PDF Table Template Filler / PDF 表格模板填充工具

## Overview / 项目概述
This project provides a simple solution to automatically fill blank cells in tables of PDFs generated from Word documents.  
It is designed to help create PDF templates with text fields, making them editable and ready for further data input or form automation.  

本项目提供一个简单工具，用于自动填充由 Word 文档转换而来的 PDF 表格中的空白单元格。  
它旨在帮助制作 PDF 模板，将空白单元格转为可编辑文本域，方便后续数据填充或表单自动化处理。  

---

## Features / 功能
- Automatically detect blank cells in PDF tables  
  自动检测 PDF 表格中的空白单元格  
- Fill blank cells with text fields for template purposes  
  将空白单元格填充为文本域，用于模板制作  
- Support PDFs generated from Word documents  
  支持由 Word 文档转换生成的 PDF 文件  
- Easy integration for automated PDF processing workflows  
  可方便地集成到自动化 PDF 处理流程中  

---

## Usage / 使用方法
1. Prepare your PDF file exported from Word  
   准备从 Word 导出的 PDF 文件  
2. Run the program and provide the PDF file path  
   运行程序，并提供 PDF 文件路径  
3. The output PDF will have all blank table cells replaced with editable text fields  
   输出的 PDF 中所有空白表格单元格将被替换为可编辑文本域  

---

## Example / 示例
Input PDF (blank table):  

| Name  | Age | Department |
|-------|-----|------------|
| Alice |     | HR         |
| Bob   | 30  |            |

Output PDF (with text fields):  

| Name  | Age [text field] | Department [text field] |
|-------|-----------------|------------------------|
| Alice | [text field]    | HR                     |
| Bob   | 30              | [text field]           |

---

## Requirements / 系统需求
- **.NET Framework 4.8**  
- **iText 7 for .NET** (for PDF processing)  
- PDF generated from Word  

---

## License / 许可
MIT License
