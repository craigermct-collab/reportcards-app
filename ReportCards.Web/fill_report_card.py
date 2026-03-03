#!/usr/bin/env python3
"""
fill_report_card.py  <template.pdf>  <values.json>  <output.pdf>

values.json format:
{
  "fields":     { "Student": "Jane Doe", ... },
  "checkboxes": { "MathematicsIEP": true, ... },
  "radios":     { "MathematicsSkill": "/G", ... }
}
"""

import sys
import json
from pypdf import PdfReader, PdfWriter
from pypdf.generic import NameObject

def fill(template_path, values_path, output_path):
    with open(values_path, "r", encoding="utf-8") as f:
        data = json.load(f)

    text_fields     = data.get("fields", {})
    checkbox_fields = data.get("checkboxes", {})
    radio_fields    = data.get("radios", {})

    reader = PdfReader(template_path)
    writer = PdfWriter()
    writer.append(reader)

    for page_idx, page in enumerate(writer.pages):
        if "/Annots" not in page:
            continue
        for annot_ref in page["/Annots"]:
            annot = annot_ref.get_object()
            if annot.get("/Subtype") != "/Widget":
                continue
            field_name = annot.get("/T")
            if not field_name:
                continue
            field_name = str(field_name)

            if field_name in text_fields:
                val = str(text_fields[field_name]) if text_fields[field_name] is not None else ""
                writer.update_page_form_field_values(writer.pages[page_idx], {field_name: val})

            if field_name in checkbox_fields:
                checked  = checkbox_fields[field_name]
                ap       = annot.get("/AP")
                on_value = "/Yes"
                if ap:
                    n = ap.get("/N")
                    if n:
                        keys = [k for k in n.keys() if k != "/Off"]
                        if keys:
                            on_value = keys[0]
                annot.update({
                    NameObject("/V"):  NameObject(on_value if checked else "/Off"),
                    NameObject("/AS"): NameObject(on_value if checked else "/Off"),
                })

            if field_name in radio_fields:
                val = radio_fields[field_name]
                writer.update_page_form_field_values(writer.pages[page_idx], {field_name: val})

    with open(output_path, "wb") as out:
        writer.write(out)

    print(f"Wrote {output_path}")

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print(f"Usage: {sys.argv[0]} <template.pdf> <values.json> <output.pdf>", file=sys.stderr)
        sys.exit(1)
    fill(sys.argv[1], sys.argv[2], sys.argv[3])
