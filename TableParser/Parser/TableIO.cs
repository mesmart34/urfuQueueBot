using System;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using System.IO;

namespace TableParser
{
    public struct SheetData
    {
        public string Name;
        public int Id;
    }

    public class TableIO
    {
        private readonly GoogleCredential _credential;
        private readonly SheetsService _service;
        private readonly string _spreadSheetId;
        private readonly string[] _scopes = { SheetsService.Scope.Spreadsheets };

        public TableIO(string spreadSheet)
        {
            _spreadSheetId = spreadSheet;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                _credential = GoogleCredential.FromStream(stream).CreateScoped(_scopes);
            }

            _service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = "urfuQueueBot"
            });
        }

        public string GetID()
        {
            return _spreadSheetId;
        }

        public IList<IList<object>> Read(string sheet)
        {
            var request = _service.Spreadsheets.Values.Get(_spreadSheetId, sheet);
            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
                return values;
            return null;
        }

        public IList<IList<object>> Read(string sheet, string range)
        {
            var request = _service.Spreadsheets.Values.Get(_spreadSheetId, sheet + "!" + range);
            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
                return values;
            return null;
        }

        public void Write(string sheet, string range, List<IList<object>> values)
        {
            var valueRange = new ValueRange();
            var address = sheet + "!" + range;
            var request = _service.Spreadsheets.Values.Update(valueRange, _spreadSheetId, address);
            valueRange.Values = values;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            request.Execute();
        }

        public void AppendRow(string sheet, IList<object> row)
        {
            var data = Read(sheet);
            Write(sheet, $"A{data.Count + 1}:B{data.Count + 1}", new List<IList<object>> { row });
        }

        public void Write(string sheet, List<IList<object>> values)
        {
            var valueRange = new ValueRange();
            var address = sheet;
            var request = _service.Spreadsheets.Values.Update(valueRange, _spreadSheetId, address);
            valueRange.Values = values;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            request.Execute();
        }

        public List<SheetData> GetAllSheets()
        {
            var request = _service.Spreadsheets.Get(_spreadSheetId);
            var response = request.Execute();
            var sheetList = new List<SheetData>();
            foreach (var sheet in response.Sheets)
            {
                var data = new SheetData();
                data.Name = sheet.Properties.Title;
                data.Id = (int)sheet.Properties.SheetId;
                sheetList.Add(data);
            }
            return sheetList;
        }

        public void DeleteSheet(int id)
        {
            var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();
            batchUpdateSpreadsheetRequest.Requests = new List<Request>();
            batchUpdateSpreadsheetRequest.Requests.Add(new Request { DeleteSheet = new DeleteSheetRequest() { SheetId = id } });
            var batchUpdateRequest = _service.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, _spreadSheetId);
            batchUpdateRequest.Execute();
        }

        public void CreateSheet(string name)
        {
            var sheets = GetAllSheets();
            foreach (var sheet in sheets)
            {
                if (sheet.Name == name)
                    return;
            }
            var addSheetRequest = new AddSheetRequest();
            addSheetRequest.Properties = new SheetProperties();
            addSheetRequest.Properties.Title = name;
            var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();
            batchUpdateSpreadsheetRequest.Requests = new List<Request>();
            batchUpdateSpreadsheetRequest.Requests.Add(new Request { AddSheet = addSheetRequest });
            var batchUpdateRequest = _service.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, _spreadSheetId);
            batchUpdateRequest.Execute();
        }

        public List<Tuple<int, int, Color>> LoadColors(string sheet)
        {
            // TODO: Защита от пустых форматированных клеток
            var colors = new List<Tuple<int, int, Color>>();
            foreach (var _sheet in GetColors(sheet).Sheets)
            {
                foreach (var data in _sheet.Data)
                {
                    for (var j = 0; j < data.RowData.Count; j++)
                    {
                        if (data.RowData[j].Values == null)
                            continue;

                        for (int i = 0; i < data.RowData[j].Values.Count; ++i)
                        {
                            var value = data.RowData[j].Values[i];
                            if (value.EffectiveFormat != null)
                                colors.Add(Tuple.Create(i, j, value.EffectiveFormat.BackgroundColor));
                        }
                    }
                }
            }
            return colors;
        }

        public Spreadsheet GetColors(string sheet)
        {
            var request = _service.Spreadsheets.Get(_spreadSheetId);
            request.Ranges = sheet;
            request.IncludeGridData = true;
            var response = request.Execute();
            return response;
        }

        public void ClearSheet(string sheetName)
        {
            var req = new BatchClearValuesRequest();
            req.Ranges = new List<string> { sheetName };
            var response = _service.Spreadsheets.Values.BatchClear(req, _spreadSheetId);
            response.Execute();
        }
    }
}
