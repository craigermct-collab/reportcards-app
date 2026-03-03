# Report Card Templates - Migration Steps

Run these in order from C:\Users\CraigM\reportcards-app\ReportCards.Web:

## 1. Create migration
dotnet ef migrations add AddReportCardTemplates

## 2. Apply to database
dotnet ef database update

## 3. Copy PDF templates into project
Copy these two files to:
  C:\Users\CraigM\reportcards-app\ReportCards.Web\ReportCardTemplates\

  - ElementaryReportCardFillable.pdf  → rename to: elementary-report-card.pdf
  - edu-elementary-progress-report-card-public-schools-editablepdf.pdf → rename to: progress-report.pdf

## 4. Copy Python fill script
Copy fill_report_card.py to:
  C:\Users\CraigM\reportcards-app\ReportCards.Web\fill_report_card.py

## 5. Seed the templates (run in SQL or via Admin page once built)
INSERT INTO ReportCardTemplates (Name, TemplateType, FileName, CreatedAt)
VALUES 
  ('Elementary Progress Report', 0, 'progress-report.pdf', GETUTCDATE()),
  ('Elementary Report Card',     1, 'elementary-report-card.pdf', GETUTCDATE());
