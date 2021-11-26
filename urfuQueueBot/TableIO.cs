using System;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using System.IO;

namespace urfuQueueBot
{
    struct SheetData
    {
        public string Name;
        public int Id;
    }

    class TableIO
    {
        private GoogleCredential credential;
        private SheetsService service;
        private string spreadSheetId;
        private string[] Scopes = { SheetsService.Scope.Spreadsheets };

        public TableIO(string spreadSheet)
        {
            spreadSheetId = spreadSheet;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "urfuQueueBot"
            });
        }

        public string GetID()
        {
            return spreadSheetId;
        }

        public IList<IList<object>> Read(string sheet)
        {
            var request = service.Spreadsheets.Values.Get(spreadSheetId, sheet);
            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
                return values;
            return null;
        }

        public IList<IList<object>> Read(string sheet, string range)
        {
            var request = service.Spreadsheets.Values.Get(spreadSheetId, sheet + "!" + range);
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
            var request = service.Spreadsheets.Values.Update(valueRange, spreadSheetId, address);
            valueRange.Values = values;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            request.Execute();
        }
        public void Write(string sheet, List<IList<object>> values)
        {
            var valueRange = new ValueRange();
            var address = sheet;
            var request = service.Spreadsheets.Values.Update(valueRange, spreadSheetId, address);
            valueRange.Values = values;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            request.Execute();
        }

        public List<SheetData> GetAllSheets()
        {
            var request = service.Spreadsheets.Get(spreadSheetId);
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
            var batchUpdateRequest = service.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, spreadSheetId);
            batchUpdateRequest.Execute();
        }

        public void CreateSheet(string name)
        {
            var sheets = GetAllSheets();
            foreach(var sheet in sheets)
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
            var batchUpdateRequest = service.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, spreadSheetId);
            batchUpdateRequest.Execute();
        }
    }
}
