﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace GoogleSheetsWrapper
{
    public abstract class BaseSheetHelper
    {
        public string SpreadsheetID { get; set; }

        public string TabName { get; private set; }

        public int? SheetID { get; set; }

        public string ServiceAccountEmail { get; set; }

        public string[] Scopes { get; set; } = { SheetsService.Scope.Spreadsheets };

        public SheetsService Service { get; private set; }

        public BaseSheetHelper(string spreadsheetID, string serviceAccountEmail, string tabName)
        {
            this.SpreadsheetID = spreadsheetID;
            this.ServiceAccountEmail = serviceAccountEmail;
            this.TabName = tabName;
        }

        public void Init(string jsonCredentials)
        {
            var credential = (ServiceAccountCredential)
                   GoogleCredential.FromJson(jsonCredentials).UnderlyingCredential;

            // Authenticate as service account to the Sheets API
            var initializer = new ServiceAccountCredential.Initializer(credential.Id)
            {
                User = this.ServiceAccountEmail,
                Key = credential.Key,
                Scopes = Scopes
            };
            credential = new ServiceAccountCredential(initializer);

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
            });

            this.Service = service;

            this.UpdateTabName(this.TabName);
        }

        /// <summary>
        /// Set the tab to the specified newTabName value
        /// </summary>
        /// <param name="newTabName"></param>
        public void UpdateTabName(string newTabName)
        {
            var spreadsheet = this.Service.Spreadsheets.Get(this.SpreadsheetID);

            var result = spreadsheet.Execute();

            Sheet sheet;

            // Lookup the sheet id for the given tab name
            if (!string.IsNullOrEmpty(newTabName))
            {
                sheet = result.Sheets.Where(s => s.Properties.Title.Equals(newTabName, StringComparison.CurrentCultureIgnoreCase)).First();
            }
            else
            {
                sheet = result.Sheets.First();
            }

            this.SheetID = sheet.Properties.SheetId;

            this.TabName = newTabName;
        }

        /// <summary>
        /// Returns a list of all tab names in the Google Spreadsheet
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllTabNames()
        {
            var spreadsheet = this.Service.Spreadsheets.Get(this.SpreadsheetID);

            var result = spreadsheet.Execute();

            var tabs = result.Sheets.Select(s => s.Properties.Title).ToList();

            return tabs;
        }

        /// <summary>
        /// Return a collection of rows for a given SheetRange input
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public IList<IList<object>> GetRows(SheetRange range)
        {
            var rangeValue = range.CanSupportA1Notation ? range.A1Notation : range.R1C1Notation;

            GetRequest request =
                    this.Service.Spreadsheets.Values.Get(this.SpreadsheetID, rangeValue);

            request.ValueRenderOption = GetRequest.ValueRenderOptionEnum.UNFORMATTEDVALUE;
            request.DateTimeRenderOption = GetRequest.DateTimeRenderOptionEnum.SERIALNUMBER;

            ValueRange response = request.Execute();
            return response.Values;
        }

        /// <summary>
        /// Return a collection of rows formatted values for a given SheetRange input
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public IList<IList<object>> GetRowsFormatted(SheetRange range)
        {
            var rangeValue = range.CanSupportA1Notation ? range.A1Notation : range.R1C1Notation;

            GetRequest request =
                    this.Service.Spreadsheets.Values.Get(this.SpreadsheetID, rangeValue);

            request.ValueRenderOption = GetRequest.ValueRenderOptionEnum.FORMATTEDVALUE;
            request.DateTimeRenderOption = GetRequest.DateTimeRenderOptionEnum.FORMATTEDSTRING;

            ValueRange response = request.Execute();
            return response.Values;
        }

        /// <summary>
        /// Deletes a specified row
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public BatchUpdateSpreadsheetResponse DeleteRow(int row)
        {
            var requests = new List<Request>();

            var request = new Request()
            {
                DeleteDimension = new DeleteDimensionRequest()
                {
                    Range = new DimensionRange()
                    {
                        Dimension = "ROWS",
                        StartIndex = row - 1,
                        EndIndex = row,
                        SheetId = this.SheetID,
                    }
                }
            };

            requests.Add(request);

            BatchUpdateSpreadsheetRequest bussr = new BatchUpdateSpreadsheetRequest();
            bussr.Requests = requests;

            var updateRequest = this.Service.Spreadsheets.BatchUpdate(bussr, this.SpreadsheetID);
            return updateRequest.Execute();
        }

        /// <summary>
        /// Inserts a blank new column using the column index as the id (NOTE: 1 is the first index for the column based index)
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public BatchUpdateSpreadsheetResponse InsertBlankColumn(int column)
        {
            if (column < 1)
            {
                throw new ArgumentException("column index value must be 1 or greater");
            }

            var requests = new List<Request>();

            var request = new Request()
            {
                InsertDimension = new InsertDimensionRequest()
                {
                    Range = new DimensionRange()
                    {
                        Dimension = "COLUMNS",
                        StartIndex = column - 1,
                        EndIndex = column,
                        SheetId = this.SheetID,
                    },
                    InheritFromBefore = column > 0,
                }
            };

            requests.Add(request);

            BatchUpdateSpreadsheetRequest bussr = new BatchUpdateSpreadsheetRequest();
            bussr.Requests = requests;

            var updateRequest = this.Service.Spreadsheets.BatchUpdate(bussr, this.SpreadsheetID);
            return updateRequest.Execute();
        }

        /// <summary>
        /// Inserts a blank new column using a letter notation (i.e. B2 as the column id)
        /// </summary>
        /// <param name="columnLetter"></param>
        /// <returns></returns>
        public BatchUpdateSpreadsheetResponse InsertBlankColumn(string columnLetter)
        {
            var columnId = SheetRange.GetColumnIDFromLetters(columnLetter);

            return this.InsertBlankColumn(columnId);
        }

        /// <summary>
        /// Inserts a new blank row
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public BatchUpdateSpreadsheetResponse InsertBlankRow(int row)
        {
            if (row < 1)
            {
                throw new ArgumentException("row index value must be 1 or greater");
            }

            var requests = new List<Request>();

            var request = new Request()
            {
                InsertDimension = new InsertDimensionRequest()
                {
                    Range = new DimensionRange()
                    {
                        Dimension = "ROWS",
                        StartIndex = row - 1,
                        EndIndex = row,
                        SheetId = this.SheetID,
                    },
                    InheritFromBefore = row > 0,
                }
            };

            requests.Add(request);

            BatchUpdateSpreadsheetRequest bussr = new BatchUpdateSpreadsheetRequest();
            bussr.Requests = requests;

            var updateRequest = this.Service.Spreadsheets.BatchUpdate(bussr, this.SpreadsheetID);
            return updateRequest.Execute();
        }

        /// <summary>
        /// Runs a collection of updates as a batch operation in a single call.
        /// 
        /// This is useful to avoid throttling limits with the Google Sheets API
        /// </summary>
        /// <param name="updates"></param>
        /// <returns></returns>
        public BatchUpdateSpreadsheetResponse BatchUpdate(List<BatchUpdateRequestObject> updates)
        {
            BatchUpdateSpreadsheetRequest bussr = new BatchUpdateSpreadsheetRequest();

            var requests = new List<Request>();

            foreach (var update in updates)
            {
                //create the update request for cells from the first row
                var updateCellsRequest = new Request()
                {
                    RepeatCell = new RepeatCellRequest()
                    {
                        Range = new GridRange()
                        {
                            SheetId = this.SheetID,
                            StartColumnIndex = update.Range.StartColumn - 1,
                            StartRowIndex = update.Range.StartRow - 1,
                            EndColumnIndex = update.Range.StartColumn,
                            EndRowIndex = update.Range.StartRow,
                        },
                        Cell = update.Data,
                        Fields = "*"
                    }
                };

                requests.Add(updateCellsRequest);
            }

            bussr.Requests = requests;

            var updateRequest = this.Service.Spreadsheets.BatchUpdate(bussr, this.SpreadsheetID);
            return updateRequest.Execute();
        }
    }
}
