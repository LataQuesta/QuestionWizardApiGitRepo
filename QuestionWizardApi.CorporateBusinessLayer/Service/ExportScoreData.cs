using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using QuestionWizardApi.CorporateData;
using QuestionWizardApi.CorporateIBusinessLayer.Interface;
using QuestionWizardApi.CorporateModel.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuestionWizardApi.CorporateBusinessLayer.Service
{
    public class ExportScoreData : IDisposable, IExport
    {
        CorporateAssessmentEntities DBEntities = new CorporateAssessmentEntities();
        const int DEFAULT_COLUMN_WIDTH = 20;
        ListtoDataTableConverter Converter = new ListtoDataTableConverter();
        QuestionService QuesSrv = new QuestionService();
        MailServiceForH1 MailSrv = new MailServiceForH1();
        UserService UserSvc = new UserService();

        ~ExportScoreData()
        {
            Dispose(false);
        }

        public DataTable RetrunUserDetails(int TestId, int ModuleId)
        {
            var objects = (from i in DBEntities.txnQuestions
                        join j in DBEntities.txnUserTestDetails
                        on i.TestId equals j.TestId
                        join k in DBEntities.txnCandidates
                        on j.UserId equals k.UserId into Candidate
                        from CandiateMap in Candidate.DefaultIfEmpty()
                        where i.TestId == TestId && i.ModuleId == ModuleId
                        select new
                        {
                            Name = CandiateMap.FirstName + " " + CandiateMap.LastName,
                            StartDate = i.ResponseAt.ToString(),
                            EndDate = j.LastModifiedAt.ToString()
                        }).OrderBy(x => x.StartDate).FirstOrDefault();

            DataTable dt = new DataTable();
            dt.TableName = "UserDetails";

            foreach (PropertyInfo property in objects.GetType().GetProperties())
            {
                dt.Columns.Add(new DataColumn(property.Name, property.PropertyType));
            }

            DataRow newRow = dt.NewRow();
            foreach (PropertyInfo property in objects.GetType().GetProperties())
            {
                newRow[property.Name] = objects.GetType().GetProperty(property.Name).GetValue(objects, null);
            }
            dt.Rows.Add(newRow);

            return dt;

        }

        public System.IO.MemoryStream GetScoreCard(int TestId)
        {
            DataSet ds = new DataSet();

            System.IO.MemoryStream Ms = null;
            
            var UserData = DBEntities.txnUserTestDetails.Join(DBEntities.txnCandidates, x => x.UserId, y => y.UserId, (x, y) => new { TestDetails = x, UserDetails = y })
                                .Where(i => i.TestDetails.TestId == TestId).Select(j => new {
                                       AssessmentId = j.UserDetails.AssessmentId.Value,
                                       Status = j.TestDetails.status
                                } ).FirstOrDefault();

            if(UserData.Status == "C" || UserData.Status == "E")
            {
                int AssessmentId = UserData.AssessmentId;

                #region QSAR
                if (AssessmentId == 1)
                {

                    var Module_1 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 1).ToList();
                    if(Module_1.Count > 0)
                    {
                        DataTable Module1 = new DataTable();
                        Module1 = Converter.ToDataTable(Module_1);
                        Module1.TableName = "Module 1";
                        ds.Tables.Add(Module1);
                    }
                    

                    var Module_3 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 3).ToList();
                    if (Module_3.Count > 0)
                    {
                        DataTable Module3 = new DataTable();
                        Module3 = Converter.ToDataTable(Module_3);
                        Module3.TableName = "Module 3";
                        ds.Tables.Add(Module3);
                    }
                        

                    var Module_4 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 4).ToList();
                    if(Module_4.Count >0)
                    {
                        DataTable Module4 = new DataTable();
                        Module4 = Converter.ToDataTable(Module_4);
                        Module4.TableName = "Module 4";
                        ds.Tables.Add(Module4);
                    }
                    

                    var DynamicMisTyping = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId).OrderByDescending(x => x.MisTypingId).Distinct().ToList();
                    int i = 0;
                    foreach (txnDynamicMisTyping DynamicMis in DynamicMisTyping)
                    {
                        var Module_2 = DBEntities.Sp_ExportSubTypeWiseScoreCard(TestId, 2, DynamicMis.MisTypeId).ToList();

                        if (Module_2.Count > 0)
                        {
                            i++;
                            string Module = "Dynamic Mistyping " + i.ToString();
                            DataTable Module2 = new DataTable();

                            Module2 = Converter.ToDataTable(Module_2);
                            Module2.TableName = Module;
                            ds.Tables.Add(Module2);
                        }
                    }

                    
                    DataTable Module5 = RetrunUserDetails(TestId, 1);
                    ds.Tables.Add(Module5);


                    Ms = CreateExcelDocumentAsStream(ds);

                }
                #endregion

                #region Qlead
                else if (AssessmentId == 3)
                {

                    var Module_5 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 5).ToList();
                    if(Module_5.Count > 0)
                    {
                        DataTable Module5 = new DataTable();
                        Module5 = Converter.ToDataTable(Module_5);
                        Module5.TableName = "Module 1";
                        ds.Tables.Add(Module5);
                    }
                   

                    var Module_7 = DBEntities.Sp_ExportRearrangeOrderScoreCard(TestId, 7).ToList();
                    if (Module_7.Count > 0)
                    {
                        DataTable Module7 = new DataTable();
                        Module7 = Converter.ToDataTable(Module_7);
                        Module7.TableName = "Module 3";
                        ds.Tables.Add(Module7);
                    }

                    var Module_8 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 8).ToList();
                    if (Module_8.Count > 0)
                    {
                        DataTable Module8 = new DataTable();
                        Module8 = Converter.ToDataTable(Module_8);
                        Module8.TableName = "Module 4";

                        Module8.Rows[0][0] = "Blindspot";
                        Module8.Rows[1][0] = "Blindspot";
                        Module8.Rows[2][0] = "Blindspot";
                        Module8.Rows[3][0] = "Fixations";
                        Module8.Rows[4][0] = "Fixations";
                        Module8.Rows[5][0] = "Fixations";

                        ds.Tables.Add(Module8);
                    }

                    var Module_9 = DBEntities.Sp_ExportRearrangeOrderScoreCard(TestId, 9).ToList();
                    if (Module_9.Count > 0)
                    {
                        DataTable Module9 = new DataTable();
                        Module9 = Converter.ToDataTable(Module_9);
                        Module9.TableName = "Module 5";
                        ds.Tables.Add(Module9);
                    }

                    var Module_10 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 10).ToList();
                    if (Module_10.Count > 0)
                    {
                        DataTable Module10 = new DataTable();
                        Module10 = Converter.ToDataTable(Module_10);
                        Module10.TableName = "Module 6";
                        ds.Tables.Add(Module10);
                    }


                    var Module_11 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 15).ToList();
                    if (Module_11.Count > 0)
                    {
                        DataTable Module11 = new DataTable();
                        Module11 = Converter.ToDataTable(Module_11);
                        Module11.TableName = "Module 7";
                        ds.Tables.Add(Module11);
                    }

                    DataTable Module_12 = RetrunUserDetails(TestId, 5);

                    ds.Tables.Add(Module_12);

                   

                    var DynamicMisTyping = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId).OrderByDescending(x => x.MisTypingId).Distinct().ToList();
                    int i = 0;
                    foreach (txnDynamicMisTyping DynamicMis in DynamicMisTyping)
                    {
                        var Module_6 = DBEntities.Sp_ExportSubTypeWiseScoreCard(TestId, 6, DynamicMis.MisTypeId).ToList();

                        if (Module_6.Count > 0)
                        {
                            i++;
                            string Module = "Dynamic Mistyping " + i.ToString();
                            DataTable Module6 = new DataTable();

                            Module6 = Converter.ToDataTable(Module_6);
                            Module6.TableName = Module;
                            ds.Tables.Add(Module6);
                        }
                    }


                    Ms = CreateExcelDocumentAsStream(ds);

                }
                #endregion

                #region Qtam
                else if (AssessmentId == 4)
                {
                    var Module_5 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 5).ToList();
                    if(Module_5.Count > 0)
                    {
                        DataTable Module5 = new DataTable();
                        Module5 = Converter.ToDataTable(Module_5);
                        Module5.TableName = "Module 1";
                        ds.Tables.Add(Module5);
                    }
                    

                    var Module_7 = DBEntities.Sp_ExportRearrangeOrderScoreCard(TestId, 7).ToList();
                    if (Module_7.Count > 0)
                    {
                        DataTable Module7 = new DataTable();
                        Module7 = Converter.ToDataTable(Module_7);
                        Module7.TableName = "Module 3";
                        ds.Tables.Add(Module7);
                    }

                    var Module_8 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 8).ToList();
                    if (Module_8.Count > 0)
                    {
                        DataTable Module8 = new DataTable();
                        Module8 = Converter.ToDataTable(Module_8);
                        Module8.TableName = "Module 4";

                        Module8.Rows[0][0] = "Blindspot";
                        Module8.Rows[1][0] = "Blindspot";
                        Module8.Rows[2][0] = "Blindspot";
                        Module8.Rows[3][0] = "Fixations";
                        Module8.Rows[4][0] = "Fixations";
                        Module8.Rows[5][0] = "Fixations";

                        ds.Tables.Add(Module8);
                    }

                    var Module_9 = DBEntities.Sp_ExportRearrangeOrderScoreCard(TestId, 9).ToList();
                    if (Module_9.Count > 0)
                    {
                        DataTable Module9 = new DataTable();
                        Module9 = Converter.ToDataTable(Module_9);
                        Module9.TableName = "Module 5";
                        ds.Tables.Add(Module9);
                    }

                    var Module_10 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 10).ToList();
                    if (Module_10 != null)
                    {
                        DataTable Module10 = new DataTable();
                        Module10 = Converter.ToDataTable(Module_10);
                        Module10.TableName = "Module 6";
                        ds.Tables.Add(Module10);
                    }


                    var Module_12 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 14).ToList();
                    if (Module_12.Count > 0)
                    {
                        DataTable Module12 = new DataTable();
                        Module12 = Converter.ToDataTable(Module_12);
                        Module12.TableName = "Module 7";
                        ds.Tables.Add(Module12);
                    }

                    var DynamicMisTyping = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId).OrderByDescending(x => x.MisTypingId).Distinct().ToList();
                    int i = 0;
                    foreach (txnDynamicMisTyping DynamicMis in DynamicMisTyping)
                    {
                        var Module_6 = DBEntities.Sp_ExportSubTypeWiseScoreCard(TestId, 6, DynamicMis.MisTypeId).ToList();

                        if (Module_6.Count > 0)
                        {
                            i++;
                            string Module = "Dynamic Mistyping " + i.ToString();
                            DataTable Module6 = new DataTable();

                            Module6 = Converter.ToDataTable(Module_6);
                            Module6.TableName = Module;
                            ds.Tables.Add(Module6);
                        }
                    }

                    DataTable Module_13 = RetrunUserDetails(TestId, 5);
                    ds.Tables.Add(Module_13);

                   

                    Ms = CreateExcelDocumentAsStream(ds);

                }
                #endregion

                #region Standard Report
                else if (AssessmentId == 5)
                {
                    var Module_5 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 5).ToList();
                    if (Module_5.Count > 0)
                    {
                        DataTable Module5 = new DataTable();
                        Module5 = Converter.ToDataTable(Module_5);
                        Module5.TableName = "Module 1";
                        ds.Tables.Add(Module5);
                    }
                    
                    var DynamicMisTyping = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId).OrderByDescending(x => x.MisTypingId).Distinct().ToList();
                    int i = 0;
                    foreach (txnDynamicMisTyping DynamicMis in DynamicMisTyping)
                    {
                        var Module_6 = DBEntities.Sp_ExportSubTypeWiseScoreCard(TestId, 6, DynamicMis.MisTypeId).ToList();

                        if (Module_6.Count > 0)
                        {
                            i++;
                            string Module = "Dynamic Mistyping " + i.ToString();
                            DataTable Module6 = new DataTable();

                            Module6 = Converter.ToDataTable(Module_6);
                            Module6.TableName = Module;
                            ds.Tables.Add(Module6);
                        }
                    }

                    DataTable Module_7 = RetrunUserDetails(TestId, 5);
                    ds.Tables.Add(Module_7);

                   

                    Ms = CreateExcelDocumentAsStream(ds);

                }
                #endregion

                #region Prenimum Report
                else if (AssessmentId == 6 || AssessmentId == 11)
                {
                    var Module_5 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 5).ToList();
                    if (Module_5.Count > 0)
                    {
                        DataTable Module5 = new DataTable();
                        Module5 = Converter.ToDataTable(Module_5);
                        Module5.TableName = "Module 1";
                        ds.Tables.Add(Module5);
                    }


                    var Module_7 = DBEntities.Sp_ExportRearrangeOrderScoreCard(TestId, 7).ToList();
                    if (Module_7.Count > 0)
                    {
                        DataTable Module7 = new DataTable();
                        Module7 = Converter.ToDataTable(Module_7);
                        Module7.TableName = "Module 3";
                        ds.Tables.Add(Module7);
                    }

                    var Module_8 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 8).ToList();
                    if (Module_8.Count > 0)
                    {
                        DataTable Module8 = new DataTable();
                        Module8 = Converter.ToDataTable(Module_8);
                        Module8.TableName = "Module 4";

                        Module8.Rows[0][0] = "Blindspot";
                        Module8.Rows[1][0] = "Blindspot";
                        Module8.Rows[2][0] = "Blindspot";
                        Module8.Rows[3][0] = "Fixations";
                        Module8.Rows[4][0] = "Fixations";
                        Module8.Rows[5][0] = "Fixations";

                        ds.Tables.Add(Module8);
                    }

                    var Module_9 = DBEntities.Sp_ExportRearrangeOrderScoreCard(TestId, 9).ToList();
                    if (Module_9.Count > 0)
                    {
                        DataTable Module9 = new DataTable();
                        Module9 = Converter.ToDataTable(Module_9);
                        Module9.TableName = "Module 5";
                        ds.Tables.Add(Module9);
                    }

                    var Module_10 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 10).ToList();
                    if (Module_10 != null)
                    {
                        DataTable Module10 = new DataTable();
                        Module10 = Converter.ToDataTable(Module_10);
                        Module10.TableName = "Module 6";
                        ds.Tables.Add(Module10);
                    }


                   

                    var DynamicMisTyping = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId).OrderByDescending(x => x.MisTypingId).Distinct().ToList();
                    int i = 0;
                    foreach (txnDynamicMisTyping DynamicMis in DynamicMisTyping)
                    {
                        var Module_6 = DBEntities.Sp_ExportSubTypeWiseScoreCard(TestId, 6, DynamicMis.MisTypeId).ToList();

                        if (Module_6.Count > 0)
                        {
                            i++;
                            string Module = "Dynamic Mistyping " + i.ToString();
                            DataTable Module6 = new DataTable();

                            Module6 = Converter.ToDataTable(Module_6);
                            Module6.TableName = Module;
                            ds.Tables.Add(Module6);
                        }
                    }

                    DataTable Module_11 = RetrunUserDetails(TestId, 5);
                    ds.Tables.Add(Module_11);

                    

                    Ms = CreateExcelDocumentAsStream(ds);

                }
                #endregion

                #region QLEAP
                else if (AssessmentId == 9)
                {
                    var Module_13 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 13).ToList();
                    if (Module_13.Count > 0)
                    {
                        DataTable Module13 = new DataTable();
                        Module13 = Converter.ToDataTable(Module_13);
                        Module13.TableName = "Module 1";
                        ds.Tables.Add(Module13);
                    }

                    var Module_11 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 15).ToList();
                    if (Module_11.Count > 0)
                    {
                        DataTable Module11 = new DataTable();
                        Module11 = Converter.ToDataTable(Module_11);
                        Module11.TableName = "Module 3";
                        ds.Tables.Add(Module11);
                    }

                    var DynamicMisTyping = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId).OrderByDescending(x => x.MisTypingId).Distinct().ToList();
                    int i = 0;
                    foreach (txnDynamicMisTyping DynamicMis in DynamicMisTyping)
                    {
                        var Module_6 = DBEntities.Sp_ExportSubTypeWiseScoreCard(TestId, 6, DynamicMis.MisTypeId).ToList();

                        if (Module_6.Count > 0)
                        {
                            i++;
                            string Module = "Dynamic Mistyping " + i.ToString();
                            DataTable Module6 = new DataTable();

                            Module6 = Converter.ToDataTable(Module_6);
                            Module6.TableName = Module;
                            ds.Tables.Add(Module6);
                        }
                    }

                    DataTable Module_7 = RetrunUserDetails(TestId, 13);
                    ds.Tables.Add(Module_7);

                    Ms = CreateExcelDocumentAsStream(ds);

                }
                #endregion

                #region QMAP
                else if (AssessmentId == 10)
                {
                    var Module_13 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 13).ToList();
                    if (Module_13.Count > 0)
                    {
                        DataTable Module13 = new DataTable();
                        Module13 = Converter.ToDataTable(Module_13);
                        Module13.TableName = "Module 1";
                        ds.Tables.Add(Module13);
                    }

                    var Module_14 = DBEntities.Sp_ExportTypeWiseScoreCard(TestId, 14).ToList();
                    if (Module_14.Count > 0)
                    {
                        DataTable Module14 = new DataTable();
                        Module14 = Converter.ToDataTable(Module_14);
                        Module14.TableName = "Module 3";
                        ds.Tables.Add(Module14);
                    }

                    var DynamicMisTyping = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == TestId).OrderByDescending(x => x.MisTypingId).Distinct().ToList();
                    int i = 0;
                    foreach (txnDynamicMisTyping DynamicMis in DynamicMisTyping)
                    {
                        var Module_6 = DBEntities.Sp_ExportSubTypeWiseScoreCard(TestId, 6, DynamicMis.MisTypeId).ToList();

                        if (Module_6.Count > 0)
                        {
                            i++;
                            string Module = "Dynamic Mistyping " + i.ToString();
                            DataTable Module6 = new DataTable();

                            Module6 = Converter.ToDataTable(Module_6);
                            Module6.TableName = Module;
                            ds.Tables.Add(Module6);
                        }
                    }

                    DataTable Module_7 = RetrunUserDetails(TestId, 13);
                    ds.Tables.Add(Module_7);

                    Ms = CreateExcelDocumentAsStream(ds);

                }
                #endregion

            }



            return Ms;
        }



        public System.IO.MemoryStream GetScoreCardDataForQLeap()
        {
            try
            {
                DataSet ds = new DataSet();

                System.IO.MemoryStream Ms = null;

                List<int> lstTestIds = new List<int>
                {
                    13123,
                    13124,
                    13125,
                    13126,
                    13127,
                    13128,
                    13129,
                    13130,
                    13131,
                    13132,
                    13133,
                    13134,
                    13135,
                    13136,
                    13137,
                    13138,
                    13139,
                    13140,
                    13141,
                    13143,
                    13144,
                    13145,
                    13146,
                    13147,
                    13148,
                    13149,
                    13150,
                    13151,
                    13152,
                    13153,
                    13154,
                    13155,
                    13156,
                    13157,
                    13158,
                    13159,
                    13160,
                    13161,
                    13162,
                    13163,
                    13164,
                    13165,
                    13166,
                    13167,
                    13168,
                    13169,
                    13170,
                    13171,
                    13142,
                    13172
                };

                List<txnUserTestDetail> lstTestDetail = DBEntities.txnCandidates.Join(
                                                DBEntities.txnUserTestDetails, x => x.UserId, y => y.UserId, (x, y) => new { Candidate = x, UserTest = y })
                                            .Where(j => j.UserTest.status == "C"  && lstTestIds.Contains(j.UserTest.TestId))
                                            .Select(i => i.UserTest).ToList();

                DataTable _dt = new DataTable();

                _dt.Columns.Add(new DataColumn { ColumnName = "TestId", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "CandidateName", DataType = typeof(string), AllowDBNull = true });
                //_dt.Columns.Add(new DataColumn { ColumnName = "AssessmentStartDate", DataType = typeof(DateTime), AllowDBNull = true });
                //_dt.Columns.Add(new DataColumn { ColumnName = "AssessmentEndDate", DataType = typeof(DateTime), AllowDBNull = true });
                //_dt.Columns.Add(new DataColumn { ColumnName = "AssessmentName", DataType = typeof(string), AllowDBNull = true });
                //_dt.Columns.Add(new DataColumn { ColumnName = "CompanyName", DataType = typeof(string), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "EnneagramType", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Business Acumen", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Drive for Results", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Integrating the Ecosystem", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Leading & Developing Teams", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Managing Self", DataType = typeof(int), AllowDBNull = true });
                
                _dt.Columns.Add(new DataColumn { ColumnName = "Building Sustainability", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Champions Change", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Customer Excellence", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Setting Vision and Strategic Purpose", DataType = typeof(int), AllowDBNull = true });
                
                _dt.Columns.Add(new DataColumn { ColumnName = "Decision Making & Problem Solving", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Entrepreneurial Mindset", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Establishes Stretch Goals", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Execution Excellence", DataType = typeof(int), AllowDBNull = true });
                
                _dt.Columns.Add(new DataColumn { ColumnName = "Networking & Influencing", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Stakeholder Relationships ", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Understanding & Navigating the Organisation", DataType = typeof(int), AllowDBNull = true });
                
                _dt.Columns.Add(new DataColumn { ColumnName = "Develops Others", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Empathy", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Inspire & Motivate", DataType = typeof(int), AllowDBNull = true });

                _dt.Columns.Add(new DataColumn { ColumnName = "Accountability & Ownership", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Ethics & Integrity", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Practices Self-development & Growth Mindset", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Tenacity", DataType = typeof(int), AllowDBNull = true });
                
                _dt.Columns.Add(new DataColumn { ColumnName = "Consistency", DataType = typeof(int), AllowDBNull = true });
                foreach (txnUserTestDetail ObjUserTest in lstTestDetail)
                {
                    CompentencyScoreCard compentencyScoreCard = new CompentencyScoreCard();
                    compentencyScoreCard.ScoreBoard = new List<ClsTypeModel>();
                    
                    DataRow _dr = _dt.NewRow();
                    _dr["TestId"] = ObjUserTest.TestId;

                    var MainType_Mistyping = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == ObjUserTest.TestId && x.Type == 1)
                                  .OrderByDescending(x => x.MisTypingId).Select(i => i.HighestType).FirstOrDefault();


                    int MainType = Convert.ToInt32(MainType_Mistyping);
                    _dr["EnneagramType"] = MainType;

                    var ObjCandidate = (from Candidate in DBEntities.txnCandidates
                                        join UserTest in DBEntities.txnUserTestDetails on Candidate.UserId equals UserTest.UserId
                                        join Assessment in DBEntities.mstAssessmentSets on Candidate.AssessmentId equals Assessment.AssessmentId into Assessment_Join
                                        from A in Assessment_Join.DefaultIfEmpty()
                                        join Company in DBEntities.mstCompanies on Candidate.CompanyId equals Company.CompanyId into Company_Join
                                        from C in Company_Join.DefaultIfEmpty()
                                        where UserTest.TestId == ObjUserTest.TestId
                                        select new
                                        {
                                            Name = Candidate.FirstName + " " + Candidate.LastName,
                                            AssessmentCreationAt = UserTest.CreatedAt,
                                            AssessmentEndAt = UserTest.LastModifiedAt,
                                            AssessmentName = A.AssessmentName,
                                            CompanyName = C.CompanyName
                                        }).FirstOrDefault();

                    _dr["CandidateName"] = ObjCandidate.Name;
                   // _dr["AssessmentStartDate"] = ObjCandidate.AssessmentCreationAt;
                    //_dr["AssessmentEndDate"] = ObjCandidate.AssessmentEndAt;
                    //_dr["AssessmentName"] = ObjCandidate.AssessmentName;
                    //_dr["CompanyName"] = ObjCandidate.CompanyName;

                    List<int> lstTypeId = new List<int>() { 177 };

                    var ScoreCard = GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency);


                    
                    //_dr["Building Sustainability"] = ScoreCard[8].Score;
                    //_dr["Champions Change"] = ScoreCard[5].Score;
                    //_dr["Customer Excellence"] = ScoreCard[18].Score;
                    //_dr["Setting Vision and Strategic Purpose"] = ScoreCard[10].Score;
                    //_dr["Decision Making & Problem Solving"] = ScoreCard[6].Score;
                    //_dr["Entrepreneurial Mindset"] = ScoreCard[17].Score;
                    //_dr["Establishes Stretch Goals"] = ScoreCard[12].Score;
                    //_dr["Execution Excellence"] = ScoreCard[16].Score;
                    //_dr["Networking & Influencing"] = ScoreCard[4].Score;
                    //_dr["Stakeholder Relationships "] = ScoreCard[3].Score;
                    //_dr["Understanding & Navigating the Organisation"] = ScoreCard[14].Score;
                    //_dr["Develops Others"] = ScoreCard[1].Score;
                    //_dr["Empathy"] = ScoreCard[11].Score;
                    //_dr["Inspire & Motivate"] = ScoreCard[15].Score;
                    //_dr["Accountability & Ownership"] = ScoreCard[2].Score;
                    //_dr["Ethics & Integrity"] = ScoreCard[9].Score;
                    //_dr["Practices Self-development & Growth Mindset"] = ScoreCard[7].Score;
                    //_dr["Tenacity"] = ScoreCard[13].Score;
                    _dr["Consistency"] = ScoreCard[0].Score;



                    var CompetencyScoreCard = (from ScoreObject in GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency)
                                               join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                               join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                              // where !lstTypeId.Contains(ScoreObject.TypeId)
                                               select new
                                               {
                                                   TypeId = MasterCluster.ClusterId,
                                                   TypeName = MasterCluster.ClusterName,
                                                   Score = CalculateOfCompetencies(ScoreObject.TypeId, ScoreObject.Score, ObjUserTest.TestId, (int)AssessmentModule.Competency),
                                                   ColorCode = ""
                                               });

                    compentencyScoreCard.ScoreBoard = (from a in CompetencyScoreCard
                                                                        join b in DBEntities.mstClusters on a.TypeId equals b.ClusterId
                                                                        group a by a.TypeId into P
                                                                        let TypeId = P.Select(x => x.TypeId).FirstOrDefault()
                                                                        let TypeName = P.Select(x => x.TypeName).FirstOrDefault()
                                                                        let ScoreByClusterWise = SumOfQleadAllCompentency(TypeId, (int)Math.Round(P.Sum(x => x.Score)))
                                                                        select new ClsTypeModel
                                                                        {
                                                                            TypeId = TypeId,
                                                                            TypeName = TypeName,
                                                                            Score = ScoreByClusterWise,
                                                                            ColorCode = GetColorCode(TypeId, ScoreByClusterWise)
                                                                        }).OrderBy(x => x.TypeName).ToList();
                    _dr["Business Acumen"] = compentencyScoreCard.ScoreBoard[0].Score;
                    _dr["Drive for Results"] = compentencyScoreCard.ScoreBoard[1].Score;
                    _dr["Integrating the Ecosystem"] = compentencyScoreCard.ScoreBoard[2].Score;
                    _dr["Leading & Developing Teams"] = compentencyScoreCard.ScoreBoard[3].Score;
                    _dr["Managing Self"] = compentencyScoreCard.ScoreBoard[4].Score;

                    var BusinessAcumenData = (from ScoreObject in GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency)
                                              join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                              join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                              where MasterCluster.ClusterId == 1
                                              select new ClsMultipleLineBarChart
                                              {
                                                  TypeId = ScoreObject.TypeId,
                                                  TypeName = ScoreObject.TypeName,
                                                  Score = CalcuateIndividualCompetencies(ScoreObject.TypeId, ObjUserTest.TestId, (int)AssessmentModule.Competency, ScoreObject.Score),
                                                  Score1 = GetNormativeScore(MainType, ScoreObject.TypeId),
                                              }).OrderBy(x => x.TypeName).ToList();

                    _dr["Building Sustainability"] = BusinessAcumenData[0].Score;
                    _dr["Champions Change"] = BusinessAcumenData[1].Score;
                    _dr["Customer Excellence"] = BusinessAcumenData[2].Score;
                    _dr["Setting Vision and Strategic Purpose"] = BusinessAcumenData[3].Score;

                    var DriveForResultsData = (from ScoreObject in GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency)
                                               join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                               join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                               where MasterCluster.ClusterId == 2
                                               select new ClsMultipleLineBarChart
                                               {
                                                   TypeId = ScoreObject.TypeId,
                                                   TypeName = ScoreObject.TypeName,
                                                   Score = CalcuateIndividualCompetencies(ScoreObject.TypeId, ObjUserTest.TestId, (int)AssessmentModule.Competency, ScoreObject.Score),
                                                   Score1 = GetNormativeScore(MainType, ScoreObject.TypeId),
                                               }).OrderBy(x => x.TypeName).ToList();

                    _dr["Decision Making & Problem Solving"] = DriveForResultsData[0].Score;
                    _dr["Entrepreneurial Mindset"] = DriveForResultsData[1].Score;
                    _dr["Establishes Stretch Goals"] = DriveForResultsData[2].Score;
                    _dr["Execution Excellence"] = DriveForResultsData[3].Score;

                    var IntegratingTheEcosystemData = (from ScoreObject in GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency)
                                                       join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                                       join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                                       where MasterCluster.ClusterId == 4
                                                       select new ClsMultipleLineBarChart
                                                       {
                                                           TypeId = ScoreObject.TypeId,
                                                           TypeName = ScoreObject.TypeName,
                                                           Score = CalcuateIndividualCompetencies(ScoreObject.TypeId, ObjUserTest.TestId, (int)AssessmentModule.Competency, ScoreObject.Score),
                                                           Score1 = GetNormativeScore(MainType, ScoreObject.TypeId),
                                                       }).OrderBy(x => x.TypeName).ToList();

                    _dr["Networking & Influencing"] = IntegratingTheEcosystemData[1].Score;
                    _dr["Stakeholder Relationships "] = IntegratingTheEcosystemData[0].Score;
                    _dr["Understanding & Navigating the Organisation"] = IntegratingTheEcosystemData[2].Score;

                    var LeadingDevelopingTalentData = (from ScoreObject in GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency)
                                                       join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                                       join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                                       where MasterCluster.ClusterId == 3
                                                       select new ClsMultipleLineBarChart
                                                       {
                                                           TypeId = ScoreObject.TypeId,
                                                           TypeName = ScoreObject.TypeName,
                                                           Score = CalcuateIndividualCompetencies(ScoreObject.TypeId, ObjUserTest.TestId, (int)AssessmentModule.Competency, ScoreObject.Score),
                                                           Score1 = GetNormativeScore(MainType, ScoreObject.TypeId),
                                                       }).OrderBy(x => x.TypeName).ToList();

                    _dr["Develops Others"] = LeadingDevelopingTalentData[0].Score;
                    _dr["Empathy"] = LeadingDevelopingTalentData[1].Score;
                    _dr["Inspire & Motivate"] = IntegratingTheEcosystemData[2].Score;

                    var ManagingSelfData = (from ScoreObject in GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency)
                                            join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                            join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                            where MasterCluster.ClusterId == 5
                                            select new ClsMultipleLineBarChart
                                            {
                                                TypeId = ScoreObject.TypeId,
                                                TypeName = ScoreObject.TypeName,
                                                Score = CalcuateIndividualCompetencies(ScoreObject.TypeId, ObjUserTest.TestId, (int)AssessmentModule.Competency, ScoreObject.Score),
                                                Score1 = GetNormativeScore(MainType, ScoreObject.TypeId),
                                            }).OrderBy(x => x.TypeName).ToList();

                    _dr["Accountability & Ownership"] = ManagingSelfData[0].Score;
                    _dr["Ethics & Integrity"] = ManagingSelfData[1].Score;
                    _dr["Practices Self-development & Growth Mindset"] = ManagingSelfData[2].Score;
                    _dr["Tenacity"] = ManagingSelfData[3].Score;

                    _dr["Tenacity"] = ManagingSelfData[3].Score;


                    _dt.Rows.Add(_dr);
                }

                ds.Tables.Add(_dt);

                Ms = CreateExcelDocumentAsStream(ds);

                return Ms;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }



        public System.IO.MemoryStream GetScoreCardDataForQLead()
        {
            try
            {
                DataSet ds = new DataSet();

                System.IO.MemoryStream Ms = null;

                List<int> lstTestIds = new List<int>
                {
                    12519,
                    12520,
                    12521,
                    12522,
                    12523,
                    12524,
                    12525,
                    12526,
                    12527,
                    12528,
                    12529,
                    12530,
                    12531,
                    12532,
                    12779,
                    12780,
                    12781,
                    12782,
                    12783
                };

                List<txnUserTestDetail> lstTestDetail = DBEntities.txnCandidates.Join(
                                                DBEntities.txnUserTestDetails, x => x.UserId, y => y.UserId, (x, y) => new { Candidate = x, UserTest = y })
                                            .Where(j => j.UserTest.status == "C" && lstTestIds.Contains(j.UserTest.TestId))
                                            .Select(i => i.UserTest).ToList();

                DataTable _dt = new DataTable();

                _dt.Columns.Add(new DataColumn { ColumnName = "TestId", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "CandidateName", DataType = typeof(string), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "AssessmentStartDate", DataType = typeof(DateTime), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "AssessmentEndDate", DataType = typeof(DateTime), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "AssessmentName", DataType = typeof(string), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "CompanyName", DataType = typeof(string), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "EnneagramType", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Business Acumen", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Drive for Results", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Integrating the Ecosystem", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Leading & Developing Teams", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Managing Self", DataType = typeof(int), AllowDBNull = true });

                _dt.Columns.Add(new DataColumn { ColumnName = "Building Sustainability", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Champions Change", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Customer Excellence", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Setting Vision and Strategic Purpose", DataType = typeof(int), AllowDBNull = true });

                _dt.Columns.Add(new DataColumn { ColumnName = "Decision Making & Problem Solving", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Entrepreneurial Mindset", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Establishes Stretch Goals", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Execution Excellence", DataType = typeof(int), AllowDBNull = true });

                _dt.Columns.Add(new DataColumn { ColumnName = "Networking & Influencing", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Stakeholder Relationships ", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Understanding & Navigating the Organisation", DataType = typeof(int), AllowDBNull = true });

                _dt.Columns.Add(new DataColumn { ColumnName = "Develops Others", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Empathy", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Inspire & Motivate", DataType = typeof(int), AllowDBNull = true });

                _dt.Columns.Add(new DataColumn { ColumnName = "Accountability & Ownership", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Ethics & Integrity", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Practices Self-development & Growth Mindset", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Tenacity", DataType = typeof(int), AllowDBNull = true });

                _dt.Columns.Add(new DataColumn { ColumnName = "Consistency", DataType = typeof(int), AllowDBNull = true });
                foreach (txnUserTestDetail ObjUserTest in lstTestDetail)
                {
                    CompentencyScoreCard compentencyScoreCard = new CompentencyScoreCard();
                    compentencyScoreCard.ScoreBoard = new List<ClsTypeModel>();

                    DataRow _dr = _dt.NewRow();
                    _dr["TestId"] = ObjUserTest.TestId;

                    var MainType_Mistyping = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == ObjUserTest.TestId && x.Type == 1)
                                  .OrderByDescending(x => x.MisTypingId).Select(i => i.HighestType).FirstOrDefault();


                    int MainType = Convert.ToInt32(MainType_Mistyping);
                    _dr["EnneagramType"] = MainType;

                    var ObjCandidate = (from Candidate in DBEntities.txnCandidates
                                        join UserTest in DBEntities.txnUserTestDetails on Candidate.UserId equals UserTest.UserId
                                        join Assessment in DBEntities.mstAssessmentSets on Candidate.AssessmentId equals Assessment.AssessmentId into Assessment_Join
                                        from A in Assessment_Join.DefaultIfEmpty()
                                        join Company in DBEntities.mstCompanies on Candidate.CompanyId equals Company.CompanyId into Company_Join
                                        from C in Company_Join.DefaultIfEmpty()
                                        where UserTest.TestId == ObjUserTest.TestId
                                        select new
                                        {
                                            Name = Candidate.FirstName + " " + Candidate.LastName,
                                            AssessmentCreationAt = UserTest.CreatedAt,
                                            AssessmentEndAt = UserTest.LastModifiedAt,
                                            AssessmentName = A.AssessmentName,
                                            CompanyName = C.CompanyName
                                        }).FirstOrDefault();

                    _dr["CandidateName"] = ObjCandidate.Name;
                    _dr["AssessmentStartDate"] = ObjCandidate.AssessmentCreationAt;
                    _dr["AssessmentEndDate"] = ObjCandidate.AssessmentEndAt;
                    _dr["AssessmentName"] = ObjCandidate.AssessmentName;
                    _dr["CompanyName"] = ObjCandidate.CompanyName;

                    List<int> lstTypeId = new List<int>() { 177 };

                    var ScoreCard = GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency);



                    //_dr["Building Sustainability"] = ScoreCard[8].Score;
                    //_dr["Champions Change"] = ScoreCard[5].Score;
                    //_dr["Customer Excellence"] = ScoreCard[18].Score;
                    //_dr["Setting Vision and Strategic Purpose"] = ScoreCard[10].Score;
                    //_dr["Decision Making & Problem Solving"] = ScoreCard[6].Score;
                    //_dr["Entrepreneurial Mindset"] = ScoreCard[17].Score;
                    //_dr["Establishes Stretch Goals"] = ScoreCard[12].Score;
                    //_dr["Execution Excellence"] = ScoreCard[16].Score;
                    //_dr["Networking & Influencing"] = ScoreCard[4].Score;
                    //_dr["Stakeholder Relationships "] = ScoreCard[3].Score;
                    //_dr["Understanding & Navigating the Organisation"] = ScoreCard[14].Score;
                    //_dr["Develops Others"] = ScoreCard[1].Score;
                    //_dr["Empathy"] = ScoreCard[11].Score;
                    //_dr["Inspire & Motivate"] = ScoreCard[15].Score;
                    //_dr["Accountability & Ownership"] = ScoreCard[2].Score;
                    //_dr["Ethics & Integrity"] = ScoreCard[9].Score;
                    //_dr["Practices Self-development & Growth Mindset"] = ScoreCard[7].Score;
                    //_dr["Tenacity"] = ScoreCard[13].Score;
                    _dr["Consistency"] = ScoreCard[0].Score;



                    var CompetencyScoreCard = (from ScoreObject in GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency)
                                               join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                               join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                               // where !lstTypeId.Contains(ScoreObject.TypeId)
                                               select new
                                               {
                                                   TypeId = MasterCluster.ClusterId,
                                                   TypeName = MasterCluster.ClusterName,
                                                   Score = CalculateOfCompetencies(ScoreObject.TypeId, ScoreObject.Score, ObjUserTest.TestId, (int)AssessmentModule.Competency),
                                                   ColorCode = ""
                                               });

                    compentencyScoreCard.ScoreBoard = (from a in CompetencyScoreCard
                                                       join b in DBEntities.mstClusters on a.TypeId equals b.ClusterId
                                                       group a by a.TypeId into P
                                                       let TypeId = P.Select(x => x.TypeId).FirstOrDefault()
                                                       let TypeName = P.Select(x => x.TypeName).FirstOrDefault()
                                                       let ScoreByClusterWise = SumOfQleadAllCompentency(TypeId, (int)Math.Round(P.Sum(x => x.Score)))
                                                       select new ClsTypeModel
                                                       {
                                                           TypeId = TypeId,
                                                           TypeName = TypeName,
                                                           Score = ScoreByClusterWise,
                                                           ColorCode = GetColorCode(TypeId, ScoreByClusterWise)
                                                       }).OrderBy(x => x.TypeName).ToList();
                    _dr["Business Acumen"] = compentencyScoreCard.ScoreBoard[0].Score;
                    _dr["Drive for Results"] = compentencyScoreCard.ScoreBoard[1].Score;
                    _dr["Integrating the Ecosystem"] = compentencyScoreCard.ScoreBoard[2].Score;
                    _dr["Leading & Developing Teams"] = compentencyScoreCard.ScoreBoard[3].Score;
                    _dr["Managing Self"] = compentencyScoreCard.ScoreBoard[4].Score;

                    var BusinessAcumenData = (from ScoreObject in GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency)
                                              join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                              join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                              where MasterCluster.ClusterId == 1
                                              select new ClsMultipleLineBarChart
                                              {
                                                  TypeId = ScoreObject.TypeId,
                                                  TypeName = ScoreObject.TypeName,
                                                  Score = CalcuateIndividualCompetencies(ScoreObject.TypeId, ObjUserTest.TestId, (int)AssessmentModule.Competency, ScoreObject.Score),
                                                  Score1 = GetNormativeScore(MainType, ScoreObject.TypeId),
                                              }).OrderBy(x => x.TypeName).ToList();

                    _dr["Building Sustainability"] = BusinessAcumenData[0].Score;
                    _dr["Champions Change"] = BusinessAcumenData[1].Score;
                    _dr["Customer Excellence"] = BusinessAcumenData[2].Score;
                    _dr["Setting Vision and Strategic Purpose"] = BusinessAcumenData[3].Score;

                    var DriveForResultsData = (from ScoreObject in GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency)
                                               join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                               join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                               where MasterCluster.ClusterId == 2
                                               select new ClsMultipleLineBarChart
                                               {
                                                   TypeId = ScoreObject.TypeId,
                                                   TypeName = ScoreObject.TypeName,
                                                   Score = CalcuateIndividualCompetencies(ScoreObject.TypeId, ObjUserTest.TestId, (int)AssessmentModule.Competency, ScoreObject.Score),
                                                   Score1 = GetNormativeScore(MainType, ScoreObject.TypeId),
                                               }).OrderBy(x => x.TypeName).ToList();

                    _dr["Decision Making & Problem Solving"] = DriveForResultsData[0].Score;
                    _dr["Entrepreneurial Mindset"] = DriveForResultsData[1].Score;
                    _dr["Establishes Stretch Goals"] = DriveForResultsData[2].Score;
                    _dr["Execution Excellence"] = DriveForResultsData[3].Score;

                    var IntegratingTheEcosystemData = (from ScoreObject in GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency)
                                                       join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                                       join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                                       where MasterCluster.ClusterId == 4
                                                       select new ClsMultipleLineBarChart
                                                       {
                                                           TypeId = ScoreObject.TypeId,
                                                           TypeName = ScoreObject.TypeName,
                                                           Score = CalcuateIndividualCompetencies(ScoreObject.TypeId, ObjUserTest.TestId, (int)AssessmentModule.Competency, ScoreObject.Score),
                                                           Score1 = GetNormativeScore(MainType, ScoreObject.TypeId),
                                                       }).OrderBy(x => x.TypeName).ToList();

                    _dr["Networking & Influencing"] = IntegratingTheEcosystemData[1].Score;
                    _dr["Stakeholder Relationships "] = IntegratingTheEcosystemData[0].Score;
                    _dr["Understanding & Navigating the Organisation"] = IntegratingTheEcosystemData[2].Score;

                    var LeadingDevelopingTalentData = (from ScoreObject in GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency)
                                                       join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                                       join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                                       where MasterCluster.ClusterId == 3
                                                       select new ClsMultipleLineBarChart
                                                       {
                                                           TypeId = ScoreObject.TypeId,
                                                           TypeName = ScoreObject.TypeName,
                                                           Score = CalcuateIndividualCompetencies(ScoreObject.TypeId, ObjUserTest.TestId, (int)AssessmentModule.Competency, ScoreObject.Score),
                                                           Score1 = GetNormativeScore(MainType, ScoreObject.TypeId),
                                                       }).OrderBy(x => x.TypeName).ToList();

                    _dr["Develops Others"] = LeadingDevelopingTalentData[0].Score;
                    _dr["Empathy"] = LeadingDevelopingTalentData[1].Score;
                    _dr["Inspire & Motivate"] = IntegratingTheEcosystemData[2].Score;

                    var ManagingSelfData = (from ScoreObject in GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.Competency)
                                            join ClusterMap in DBEntities.txnClusterMapToCompetencies on ScoreObject.TypeId equals ClusterMap.TypeId
                                            join MasterCluster in DBEntities.mstClusters on ClusterMap.ClusterId equals MasterCluster.ClusterId
                                            where MasterCluster.ClusterId == 5
                                            select new ClsMultipleLineBarChart
                                            {
                                                TypeId = ScoreObject.TypeId,
                                                TypeName = ScoreObject.TypeName,
                                                Score = CalcuateIndividualCompetencies(ScoreObject.TypeId, ObjUserTest.TestId, (int)AssessmentModule.Competency, ScoreObject.Score),
                                                Score1 = GetNormativeScore(MainType, ScoreObject.TypeId),
                                            }).OrderBy(x => x.TypeName).ToList();

                    _dr["Accountability & Ownership"] = ManagingSelfData[0].Score;
                    _dr["Ethics & Integrity"] = ManagingSelfData[1].Score;
                    _dr["Practices Self-development & Growth Mindset"] = ManagingSelfData[2].Score;
                    _dr["Tenacity"] = ManagingSelfData[3].Score;

                    //_dr["Tenacity"] = ManagingSelfData[3].Score;


                    _dt.Rows.Add(_dr);
                }

                ds.Tables.Add(_dt);

                Ms = CreateExcelDocumentAsStream(ds);

                return Ms;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }


        public int CalcuateIndividualCompetencies(int TypeId, int TestId, int SetId, int Score)
        {
            int NumberOfQuestion = DBEntities.txnQuestions.Join(DBEntities.mstQuestions,
                                                   x => x.QuestionId, y => y.QuestionId, (x, y) => new { x, y })
                                                   .Where(i => i.x.TestId == TestId && i.x.ModuleId == SetId && i.y.TypeId == TypeId).Count();

            NumberOfQuestion = NumberOfQuestion * 10;

            decimal CalculateScore = decimal.Divide(Score, NumberOfQuestion) * 100;
            return (int)Math.Round(decimal.ToDouble(CalculateScore));

        }

        public int GetNormativeScore(int MainTypeId, int TypeId)
        {
            string ColumnName = "Type" + MainTypeId;
            var NormativeData = DBEntities.Mst_QMP_Normatives.Where(x => x.TypeId == TypeId).Select(y => ColumnName == "Type1" ? y.Type1 :
                                                                                                        ColumnName == "Type2" ? y.Type2 :
                                                                                                        ColumnName == "Type3" ? y.Type3 :
                                                                                                        ColumnName == "Type4" ? y.Type4 :
                                                                                                        ColumnName == "Type5" ? y.Type5 :
                                                                                                        ColumnName == "Type6" ? y.Type6 :
                                                                                                        ColumnName == "Type7" ? y.Type7 :
                                                                                                        ColumnName == "Type8" ? y.Type8 :
                                                                                                        ColumnName == "Type9" ? y.Type9 : null).FirstOrDefault();
            int NormativeScore = Convert.ToInt32(NormativeData);
            return NormativeScore;
        }
        public string GetColorCode(int? TypeId, int Score)
        {
            if (TypeId != null)
            {
                switch (TypeId)
                {
                    case 1:
                        return "#2386B8";
                    case 29:
                        return "#F2D949";
                    case 2:
                        return "#92C4D6";
                    case 30:
                        return "#C0B6AA";
                    case 3:
                        return "#61A0B2";
                    case 31:
                        return "#6C6F74";
                    case 4:
                        return "#3D7D92";
                    case 32:
                        return "#3B3A40";
                    case 5:
                        return "#235F75";
                    default:
                        return "";
                }
            }
            else
            {
                if (Score >= 70)
                {
                    return "#74C25C";
                }
                else if (Score >= 40)
                {
                    return "#FFA500";
                }
                else if (Score < 40)
                {
                    return "#FF0000";
                }
                else
                {
                    return "";
                }
            }
        }
        public int SumOfQleadAllCompentency(int ClusterId, int Score)
        {
            int SumOfAllCompent = DBEntities.txnClusterMapToCompetencies.Where(x => x.ClusterId == ClusterId).Sum(x => x.Weightage.Value);

            decimal CalculateScore = decimal.Divide(Score, SumOfAllCompent) * 100;

            return (int)Math.Round(decimal.ToDouble(CalculateScore));

        }
        public double CalculateOfCompetencies(int TypeId, int SumOfScore, int TestId, int SetId)
        {

            string ColourCode = string.Empty;
            double FinalCalcuationOfScore = 0;
            int NumberOfQuestion = 0;
            int? WeightOfCompetencies;
            CandidateBM UserModel = UserSvc.GetCandidateData(TestId);

            if (SetId == (int)AssessmentModule.Competency)
            {
                NumberOfQuestion = DBEntities.txnQuestions.Join(DBEntities.mstQuestions,
                                                   x => x.QuestionId, y => y.QuestionId, (x, y) => new { x, y })
                                                   .Where(i => i.x.TestId == TestId && i.x.ModuleId == SetId && i.y.TypeId == TypeId).Count();

                NumberOfQuestion = NumberOfQuestion * 10;

                WeightOfCompetencies = DBEntities.txnClusterMapToCompetencies.Where(x => x.TypeId == TypeId).Select(x => x.Weightage).FirstOrDefault();
                decimal CalculateScore = decimal.Divide(SumOfScore, NumberOfQuestion) * WeightOfCompetencies.Value;
                FinalCalcuationOfScore = decimal.ToDouble(CalculateScore);
            }
            else if (SetId == (int)AssessmentModule.QTamCompetency)
            {
                NumberOfQuestion = DBEntities.txnQuestions.Join(DBEntities.mstQuestions,
                                                     x => x.QuestionId, y => y.QuestionId, (x, y) => new { x, y })
                                                     .Where(i => i.x.TestId == TestId && i.x.ModuleId == SetId && i.y.TypeId == TypeId).Count();

                NumberOfQuestion = NumberOfQuestion * 10;

                WeightOfCompetencies = DBEntities.txnClusterMapToCompetencies.Where(x => x.TypeId == TypeId).Select(x => x.WeightageScore).FirstOrDefault();

                //  decimal CalculateScore = WeightOfCompetencies.Value * SumOfScore / NumberOfQuestion; //decimal.Divide(Score, SumOfAllCompent) * 100;
                //  FinalCalcuationOfScore = (int)Math.Round(decimal.ToDouble(CalculateScore));

                decimal CalculateScore = decimal.Divide(SumOfScore, NumberOfQuestion) * WeightOfCompetencies.Value;
                FinalCalcuationOfScore = decimal.ToDouble(CalculateScore);
            }
            //else if(SetId == (int)AssessmentModule.QTamCompetency)
            //{
            //    NumberOfQuestion = DBEntities.txnQuestions.Join(DBEntities.mstQuestions,
            //                                       x => x.QuestionId, y => y.QuestionId, (x, y) => new { x, y })
            //                                       .Where(i => i.x.TestId == TestId && i.x.ModuleId == SetId && i.y.TypeId == TypeId).Count();

            //    NumberOfQuestion = NumberOfQuestion * 10;

            //    decimal CalculateScore = decimal.Divide(SumOfScore, NumberOfQuestion) * 100;

            //    FinalCalcuationOfScore = decimal.ToDouble(CalculateScore);
            //}

            return FinalCalcuationOfScore;
        }
        public List<ClsTypeModel> GetTypeWiseScoreBoard(int TestId, int SetId)
        {

            try
            {
                List<ClsTypeModel> lstTypeWiseScoreCard = new List<ClsTypeModel>();

                lstTypeWiseScoreCard = DBEntities.usp_TypeWiseScoreBoard(testId: TestId, sETId: SetId).Select(x => new ClsTypeModel
                {
                    TypeId = x.TypeId.HasValue ? x.TypeId.Value : 0,
                    TypeName = x.TypeName,
                    Score = x.ImpactScore.HasValue ? x.ImpactScore.Value : 0,
                    ColorCode = x.colorCode
                }).ToList();

                if (SetId != (int)AssessmentModule.H1PartAAptitude)
                {
                    var ScoreCardName = DBEntities.usp_GetScoreCardName(testId: TestId, setId: SetId).ToList();

                    foreach (var Name in ScoreCardName)
                    {
                        if (!(lstTypeWiseScoreCard.Any(x => x.TypeId == Name.Id)))
                        {
                            lstTypeWiseScoreCard.Add(new ClsTypeModel
                            {
                                TypeId = Name.Id.Value,
                                TypeName = Name.Name,
                                Score = 0
                            });
                        }
                    }
                }


                return lstTypeWiseScoreCard;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public System.IO.MemoryStream GetScoreCardDataForQSSER()
        {
            int _errorTestId = 0;
            try
            {
                DataSet ds = new DataSet();

                System.IO.MemoryStream Ms = null;
                List<int> lstTestIds = new List<int>
                {
                    12922,
                    12923,
                    12924,
                    12925,
                    12926,
                    12927,
                    12928,
                    12929,
                    12930,
                    12931,
                    12932,
                    12933,
                    12934,
                    12935,
                    12936,
                    12937,
                    12938,
                    12939,
                    12940,
                    12941,
                    12942,
                    12943,
                    12944

                };


                List<txnUserTestDetail> lstTestDetail = DBEntities.txnCandidates.Join(
                                                DBEntities.txnUserTestDetails, x => x.UserId, y => y.UserId, (x, y) => new { Candidate = x, UserTest = y })
                                            .Where(j=>j.UserTest.status == "C" && j.Candidate.AssessmentId == 1 && lstTestIds.Contains(j.UserTest.TestId))
                                            .Select(i => i.UserTest).ToList();
                DataTable _dt = new DataTable();

                _dt.Columns.Add(new DataColumn { ColumnName = "TestId", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "CandidateName", DataType = typeof(string), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "AssessmentStartDate", DataType = typeof(DateTime), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "AssessmentEndDate", DataType = typeof(DateTime), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "AssessmentName", DataType = typeof(string), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "CompanyName", DataType = typeof(string), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "EnneagramType", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Logical Aptitude", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Numerical Aptitude", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Verbal Aptitude", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Process orientation", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Accountability and Commitment", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Influencing And Negotiation", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Achievement orientation", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Resilience", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Social Connectivity", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Customer Orientation", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Focus on Self Development", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Consistency status", DataType = typeof(string), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Service Orientation", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Consistency", DataType = typeof(int), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Suitability Status", DataType = typeof(string), AllowDBNull = true });
                _dt.Columns.Add(new DataColumn { ColumnName = "Suitability Score", DataType = typeof(string), AllowDBNull = true });

                foreach (txnUserTestDetail ObjUserTest in lstTestDetail)
                {
                    DataRow _dr = _dt.NewRow();
                    _dr["TestId"] = ObjUserTest.TestId;

                    _errorTestId = ObjUserTest.TestId;

                    var MainType_Mistyping = DBEntities.txnDynamicMisTypings.Where(x => x.TestId == ObjUserTest.TestId && x.Type == 1)
                                   .OrderByDescending(x => x.MisTypingId).Select(i => i.HighestType).FirstOrDefault();
                   

                    int MainType = Convert.ToInt32(MainType_Mistyping);
                    _dr["EnneagramType"] = MainType;


                    int? SumOfOverallScore = SumOfAllModule_Qsssr(ObjUserTest.TestId, MainType);
                    if(SumOfOverallScore != null)
                    {
                        string SuitabilityStatus = string.Empty;


                        if (SumOfOverallScore > 0 && SumOfOverallScore <= 40)
                        {
                            SuitabilityStatus = "Low Suitability";
                        }
                        else if (SumOfOverallScore >= 41 && SumOfOverallScore <= 55)
                        {
                            SuitabilityStatus = "Moderate Suitability";
                        }
                        else if (SumOfOverallScore >= 56 && SumOfOverallScore <= 69)
                        {
                            SuitabilityStatus = "Above Average Suitability";
                        }
                        else if (SumOfOverallScore >= 70)
                        {
                            SuitabilityStatus = "High Suitability";
                        }
                        _dr["Suitability Status"] = SuitabilityStatus;
                        _dr["Suitability Score"] = SumOfOverallScore.ToString() + "%";
                    }
                    else
                    {
                        _dr["Suitability Status"] = "";
                        _dr["Suitability Score"] = "";
                    }
                    

                    var ObjCandidate = (from Candidate in DBEntities.txnCandidates
                                        join UserTest in DBEntities.txnUserTestDetails on Candidate.UserId equals UserTest.UserId
                                        join Assessment in DBEntities.mstAssessmentSets on Candidate.AssessmentId equals Assessment.AssessmentId into Assessment_Join
                                        from A in Assessment_Join.DefaultIfEmpty()
                                        join Company in DBEntities.mstCompanies on Candidate.CompanyId equals Company.CompanyId into Company_Join
                                        from C in Company_Join.DefaultIfEmpty()
                                        where UserTest.TestId == ObjUserTest.TestId
                                        select new
                                        {
                                            Name = Candidate.FirstName + " " + Candidate.LastName,
                                            AssessmentCreationAt = UserTest.CreatedAt,
                                            AssessmentEndAt = UserTest.LastModifiedAt,
                                            AssessmentName = A.AssessmentName,
                                            CompanyName = C.CompanyName
                                        }).FirstOrDefault();

                    _dr["CandidateName"] = ObjCandidate.Name;
                    _dr["AssessmentStartDate"] = ObjCandidate.AssessmentCreationAt;
                    _dr["AssessmentEndDate"] = ObjCandidate.AssessmentEndAt;
                    _dr["AssessmentName"] = ObjCandidate.AssessmentName;
                    _dr["CompanyName"] = ObjCandidate.CompanyName;

                    string ConsistencyStatus = QuesSrv.GetConsistencyStatus(ObjUserTest.TestId);
                    _dr["Consistency status"] = ConsistencyStatus;


                    var TempApptitudeScoreCard = (from obj in QuesSrv.GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.H1PartAAptitude)
                                                  select new
                                                  {
                                                      TypeName = obj.TypeName,
                                                      TypeId = obj.TypeId,
                                                      Score = obj.Score
                                                  }).OrderBy(x => x.TypeId).ToList();

                    _dr["Logical Aptitude"] = TempApptitudeScoreCard.Where(x => x.TypeId == 1).Select(y => y.Score).FirstOrDefault();
                    _dr["Numerical Aptitude"] = TempApptitudeScoreCard.Where(x => x.TypeId == 2).Select(y => y.Score).FirstOrDefault();
                    _dr["Verbal Aptitude"] = TempApptitudeScoreCard.Where(x => x.TypeId == 3).Select(y => y.Score).FirstOrDefault();

                    


                    var CompentenciesScoreCard = (from obj in QuesSrv.GetTypeWiseScoreBoard(ObjUserTest.TestId, (int)AssessmentModule.H1PartACompetency)
                                                  select new
                                                  {
                                                      TypeName = obj.TypeName,
                                                      TypeId = obj.TypeId,
                                                      PercentageScore = QuesSrv.GetCompetenciesCalculation(obj.TypeId, obj.Score, ObjUserTest.TestId, (int)AssessmentModule.H1PartACompetency),
                                                  }).OrderBy(x => x.TypeId).ToList();

                    _dr["Process orientation"] = CompentenciesScoreCard.Where(x => x.TypeId == 53 || x.TypeId == 195).Select(y => y.PercentageScore).FirstOrDefault();
                    _dr["Accountability and Commitment"] = CompentenciesScoreCard.Where(x => x.TypeId == 54 || x.TypeId == 196).Select(y => y.PercentageScore).FirstOrDefault();
                    _dr["Influencing And Negotiation"] = CompentenciesScoreCard.Where(x => x.TypeId == 55 || x.TypeId == 197).Select(y => y.PercentageScore).FirstOrDefault();
                    _dr["Achievement orientation"] = CompentenciesScoreCard.Where(x => x.TypeId == 56 || x.TypeId == 198).Select(y => y.PercentageScore).FirstOrDefault();
                    _dr["Resilience"] = CompentenciesScoreCard.Where(x => x.TypeId == 57 || x.TypeId == 199).Select(y => y.PercentageScore).FirstOrDefault();
                    _dr["Social Connectivity"] = CompentenciesScoreCard.Where(x => x.TypeId == 58 || x.TypeId == 200).Select(y => y.PercentageScore).FirstOrDefault();
                    _dr["Customer Orientation"] = CompentenciesScoreCard.Where(x => x.TypeId == 59).Select(y => y.PercentageScore).FirstOrDefault();
                    _dr["Focus on Self Development"] = CompentenciesScoreCard.Where(x => x.TypeId == 60 || x.TypeId == 202).Select(y => y.PercentageScore).FirstOrDefault();
                    _dr["Consistency"] = CompentenciesScoreCard.Where(x => x.TypeId == 62).Select(y => y.PercentageScore).FirstOrDefault();
                    _dr["Service Orientation"] = CompentenciesScoreCard.Where(x => x.TypeId == 201).Select(y => y.PercentageScore).FirstOrDefault();
                    _dt.Rows.Add(_dr);
                }

                ds.Tables.Add(_dt);

                Ms = CreateExcelDocumentAsStream(ds);

                return Ms;
            }
            catch(Exception ex)
            {
                Console.WriteLine(_errorTestId);

                throw;
            }
        }

        private int? SumOfAllModule_Qsssr(int TestId, int MainType)
        {
            var TempCompentenciesScoreCard = QuesSrv.GetTypeWiseScoreBoard(TestId, (int)AssessmentModule.H1PartACompetency);

            List<int> lst = new List<int> { 53, 54, 55, 56, 57, 58, 59, 60 };

            if(!TempCompentenciesScoreCard.Any(x=> lst.Contains(x.TypeId)))
            {
                int SumOfCompetency = CalculationOf_QSSSR_Competency(TempCompentenciesScoreCard, TestId, (int)AssessmentModule.H1PartACompetency);
                int WeightedCompetency = 50;

                decimal CalculateOfCompetency = decimal.Divide(SumOfCompetency, 100) * WeightedCompetency;
                int RoundOfCompetency = CalculateOfCompetency == 0 ? 0 : (int)Math.Round(CalculateOfCompetency, MidpointRounding.AwayFromZero);


                int SumOfApptitude = QuesSrv.GetTypeWiseScoreBoard(TestId, (int)AssessmentModule.H1PartAAptitude).Sum(x => x.Score);
                int WeightedApptitude = 20;

                decimal CalculateOfAptitude = decimal.Divide(SumOfApptitude, 30) * WeightedApptitude;
                int RoundOfAptitudeScore = CalculateOfAptitude == 0 ? 0 : (int)Math.Round(CalculateOfAptitude, MidpointRounding.AwayFromZero);


                int SumOfTypeWiseScore = GetQssrTypeWiseScore(MainType);
                int WeightedType = 30;

                decimal CalculateOfType = decimal.Divide(SumOfTypeWiseScore, 30) * WeightedType;
                int RoundOfType = CalculateOfType == 0 ? 0 : (int)Math.Round(CalculateOfType, MidpointRounding.AwayFromZero);

                return RoundOfCompetency + RoundOfAptitudeScore + RoundOfType;
            }

            return null;
            
        }
        private int CalculationOf_QSSSR_Competency(List<ClsTypeModel> LstOfScore, int TestId, int ModuleId)
        {
            var SumOfAll_QSSSR_Competency = (from obj in LstOfScore
                                             where obj.TypeId != 62
                                             select new
                                             {
                                                 TypeName = obj.TypeName,
                                                 TypeId = obj.TypeId,
                                                 PercentageScore = CalcuateEachQSSSRComptency(TestId, ModuleId, obj.TypeId, obj.Score),
                                             }).ToList();

            return SumOfAll_QSSSR_Competency.Sum(x => x.PercentageScore);

        }

        private int CalcuateEachQSSSRComptency(int TestId, int ModuleId, int TypeId, int SumOfScore)
        {
            int FinalCalcuationOfScore = 0;

            int NumberOfQuestion = DBEntities.txnQuestions.Join(DBEntities.mstQuestions,
                                                      x => x.QuestionId, y => y.QuestionId, (x, y) => new { x, y })
                                                      .Where(i => i.x.TestId == TestId && i.x.ModuleId == ModuleId && i.y.TypeId == TypeId).Count();

            NumberOfQuestion = NumberOfQuestion * 10;

            decimal CalculateScore = decimal.Divide(SumOfScore, NumberOfQuestion) * GetQssrCompetencyWiseScore(TypeId);

            FinalCalcuationOfScore = CalculateScore == 0 ? 0 : (int)Math.Round(CalculateScore, MidpointRounding.AwayFromZero);
            return FinalCalcuationOfScore;
        }

        private int GetQssrCompetencyWiseScore(int CompentencyId)
        {
            Dictionary<int, int> CompetencyWiseScore = new Dictionary<int, int>()
            {
                {195,10},{196,20},{197,10},{198,25},{199,10},{200,5},{201,10},{202,10}
            };

            return CompetencyWiseScore[CompentencyId];
        }
        private int GetQssrTypeWiseScore(int TypeId)
        {
            Dictionary<int, int> TypeWiseScore = new Dictionary<int, int>()
            {
                {1,20},{2,22},{3,30},{4,15},{5,16},{6,24},{7,28},{8,26},{9,18}
            };

            return TypeWiseScore[TypeId];
        }

        public string GetName(int TestId)
        {
            var data = DBEntities.txnUserTestDetails.Join(DBEntities.txnCandidates, i => i.UserId, j => j.UserId, (i, j) => new { i, j }).Where(x => x.i.TestId == TestId)
                        .Select(x => new
                        {
                            name = x.j.FirstName + " " + x.j.LastName
                        }).FirstOrDefault();
            
            return data.name.ToString();
        }

        public System.IO.MemoryStream CreateExcelDocumentAsStream(DataSet ds)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();

            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
            {
                WriteExcelFile(ds, document);
                
            }
            stream.Flush();
            stream.Position = 0;

            return stream;
        }
        private static void WriteExcelFile(DataSet ds, SpreadsheetDocument spreadsheet)
        {
            spreadsheet.AddWorkbookPart();
            spreadsheet.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

            DefinedNames definedNamesCol = new DefinedNames();

            //  My thanks to James Miera for the following line of code (which prevents crashes in Excel 2010)
            spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

            //  If we don't add a "WorkbookStylesPart", OLEDB will refuse to connect to this .xlsx file !
            WorkbookStylesPart workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
            workbookStylesPart.Stylesheet = GenerateStyleSheet();
            workbookStylesPart.Stylesheet.Save();


            //  Loop through each of the DataTables in our DataSet, and create a new Excel Worksheet for each.
            uint worksheetNumber = 1;
            Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            foreach (DataTable dt in ds.Tables)
            {
                //  For each worksheet you want to create
                string worksheetName = dt.TableName;

                //  Create worksheet part, and add it to the sheets collection in workbook
                WorksheetPart newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
                Sheet sheet = new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart), SheetId = worksheetNumber, Name = worksheetName };

                // If you want to define the Column Widths for a Worksheet, you need to do this *before* appending the SheetData
                // http://social.msdn.microsoft.com/Forums/en-US/oxmlsdk/thread/1d93eca8-2949-4d12-8dd9-15cc24128b10/


                sheets.Append(sheet);

                //  Append this worksheet's data to our Workbook, using OpenXmlWriter, to prevent memory problems
                WriteDataTableToExcelWorksheet(dt, newWorksheetPart, definedNamesCol);

                worksheetNumber++;
            }
            spreadsheet.WorkbookPart.Workbook.Append(definedNamesCol);
            spreadsheet.WorkbookPart.Workbook.Save();

        }

        private static void WriteDataTableToExcelWorksheet(DataTable dt, WorksheetPart worksheetPart, DefinedNames definedNamesCol)
        {
            OpenXmlWriter writer = OpenXmlWriter.Create(worksheetPart, Encoding.ASCII);
            writer.WriteStartElement(new Worksheet());

            //  To demonstrate how to set column-widths in Excel, here's how to set the width of all columns to our default of "25":
            UInt32 inx = 1;
            writer.WriteStartElement(new Columns());
            foreach (DataColumn dc in dt.Columns)
            {
                writer.WriteElement(new Column { Min = inx, Max = inx, CustomWidth = true, Width = DEFAULT_COLUMN_WIDTH });
                inx++;
            }
            writer.WriteEndElement();


            writer.WriteStartElement(new SheetData());

            string cellValue = "";
            string cellReference = "";

            //  Create a Header Row in our Excel file, containing one header for each Column of data in our DataTable.
            //
            //  We'll also create an array, showing which type each column of data is (Text or Numeric), so when we come to write the actual
            //  cells of data, we'll know if to write Text values or Numeric cell values.
            int numberOfColumns = dt.Columns.Count;
            bool[] IsIntegerColumn = new bool[numberOfColumns];
            bool[] IsFloatColumn = new bool[numberOfColumns];
            bool[] IsDateColumn = new bool[numberOfColumns];

            string[] excelColumnNames = new string[numberOfColumns];
            for (int n = 0; n < numberOfColumns; n++)
                excelColumnNames[n] = GetExcelColumnName(n);

            //
            //  Create the Header row in our Excel Worksheet
            //  We'll set the row-height to 20px, and (using the "AppendHeaderTextCell" function) apply some formatting to the cells.
            //
            uint rowIndex = 1;

            writer.WriteStartElement(new Row { RowIndex = rowIndex, Height = 20, CustomHeight = true });
            for (int colInx = 0; colInx < numberOfColumns; colInx++)
            {
                DataColumn col = dt.Columns[colInx];
                AppendHeaderTextCell(excelColumnNames[colInx] + "1", col.ColumnName, writer);
                IsIntegerColumn[colInx] = (col.DataType.FullName.StartsWith("System.Int"));
                IsFloatColumn[colInx] = (col.DataType.FullName == "System.Decimal") || (col.DataType.FullName == "System.Double") || (col.DataType.FullName == "System.Single");
                IsDateColumn[colInx] = (col.DataType.FullName == "System.DateTime");

                //  Uncomment the following lines, for an example of how to create some Named Ranges in your Excel file
#if FALSE
                //  For each column of data in this worksheet, let's create a Named Range, showing where there are values in this column
                //       eg  "NamedRange_UserID"  = "Drivers!$A2:$A6"
                //           "NamedRange_Surname" = "Drivers!$B2:$B6"
                string columnHeader = col.ColumnName.Replace(" ", "_");
                string NamedRange = string.Format("{0}!${1}2:${2}{3}", worksheetName, excelColumnNames[colInx], excelColumnNames[colInx], dt.Rows.Count + 1);
                DefinedName definedName = new DefinedName() { 
                    Name = "NamedRange_" + columnHeader,
                    Text = NamedRange 
                };       
                definedNamesCol.Append(definedName);        
#endif
            }
            writer.WriteEndElement();   //  End of header "Row"

            //
            //  Now, step through each row of data in our DataTable...
            //
            double cellFloatValue = 0;
            int cellIntValue = 0;
            CultureInfo ci = new CultureInfo("en-US");
            foreach (DataRow dr in dt.Rows)
            {
                // ...create a new row, and append a set of this row's data to it.
                ++rowIndex;

                writer.WriteStartElement(new Row { RowIndex = rowIndex });

                for (int colInx = 0; colInx < numberOfColumns; colInx++)
                {
                    cellValue = dr.ItemArray[colInx].ToString();
                    cellValue = ReplaceHexadecimalSymbols(cellValue);
                    cellReference = excelColumnNames[colInx] + rowIndex.ToString();

                    // Create cell with data
                    if (IsIntegerColumn[colInx] || IsFloatColumn[colInx])
                    {
                        //  For numeric cells without any decimal places.
                        //  If this numeric value is NULL, then don't write anything to the Excel file.
                        cellFloatValue = 0;
                        bool bIncludeDecimalPlaces = IsFloatColumn[colInx];
                        if (double.TryParse(cellValue, out cellFloatValue))
                        {
                            cellValue = cellFloatValue.ToString(ci);
                            AppendNumericCell(cellReference, cellValue, bIncludeDecimalPlaces, writer);
                        }
                    }
                    else if (IsDateColumn[colInx])
                    {
                        //  For date values, we save the value to Excel as a number, but need to set the cell's style to format
                        //  it as either a date or a date-time.
                        DateTime dateValue;
                        if (DateTime.TryParse(cellValue, out dateValue))
                        {
                            AppendDateCell(cellReference, dateValue, writer);
                        }
                        else
                        {
                            //  This should only happen if we have a DataColumn of type "DateTime", but this particular value is null/blank.
                            AppendTextCell(cellReference, cellValue, writer);
                        }
                    }
                    else if(colInx == 3)
                    {
                        cellIntValue = 0;
                        bool bIncludeDecimalPlaces = IsIntegerColumn[colInx];
                        if (int.TryParse(cellValue, out cellIntValue))
                        {
                            cellValue = cellIntValue.ToString(ci);
                            AppendNumericCell(cellReference, cellValue, bIncludeDecimalPlaces, writer);
                        }
                    }
                    else
                    {
                        //  For text cells, just write the input data straight out to the Excel file.
                        AppendTextCell(cellReference, cellValue, writer);
                    }
                }
                writer.WriteEndElement(); //  End of Row
            }
            writer.WriteEndElement(); //  End of SheetData
            writer.WriteEndElement(); //  End of worksheet

            writer.Close();
        }


        //  Convert a zero-based column index into an Excel column reference  (A, B, C.. Y, Z, AA, AB, AC... AY, AZ, BA, BB..)
        public static string GetExcelColumnName(int columnIndex)
        {
            //  Convert a zero-based column index into an Excel column reference  (A, B, C.. Y, Z, AA, AB, AC... AY, AZ, BA, BB..)
            //  eg GetExcelColumnName(0) should return "A"
            //     GetExcelColumnName(1) should return "B"
            //     GetExcelColumnName(25) should return "Z"
            //     GetExcelColumnName(26) should return "AA"
            //     GetExcelColumnName(27) should return "AB"
            //     GetExcelColumnName(701) should return "ZZ"
            //     GetExcelColumnName(702) should return "AAA"
            //     GetExcelColumnName(1999) should return "BXX"
            //     ..etc..

            int firstInt = columnIndex / 676;
            int secondInt = (columnIndex % 676) / 26;
            if (secondInt == 0)
            {
                secondInt = 26;
                firstInt = firstInt - 1;
            }
            int thirdInt = (columnIndex % 26);

            char firstChar = (char)('A' + firstInt - 1);
            char secondChar = (char)('A' + secondInt - 1);
            char thirdChar = (char)('A' + thirdInt);

            if (columnIndex < 26)
                return thirdChar.ToString();

            if (columnIndex < 702)
                return string.Format("{0}{1}", secondChar, thirdChar);

            return string.Format("{0}{1}{2}", firstChar, secondChar, thirdChar);
        }

        private static Stylesheet GenerateStyleSheet()
        {
            //  If you want certain Excel cells to have a different Format, color, border, fonts, etc, then you need to define a "CellFormats" records containing 
            //  these attributes, then assign that style number to your cell.
            //
            //  For example, we'll define "Style # 3" with the attributes we'd like for our header row (Row #1) on each worksheet, where the text is a bit bigger,
            //  and is white text on a dark-gray background.
            // 
            //  NB: The NumberFormats from 0 to 163 are hardcoded in Excel (described in the following URL), and we'll define a couple of custom number formats below.
            //  https://msdn.microsoft.com/en-us/library/documentformat.openxml.spreadsheet.numberingformat.aspx
            //  http://lateral8.com/articles/2010/6/11/openxml-sdk-20-formatting-excel-values.aspx
            //
            uint iExcelIndex = 164;

            return new Stylesheet(
                new NumberingFormats(
                    //  
                    new NumberingFormat()                                                  // Custom number format # 164: especially for date-times
                    {
                        NumberFormatId = UInt32Value.FromUInt32(iExcelIndex++),
                        FormatCode = StringValue.FromString("dd/MMM/yyyy hh:mm:ss")
                    },
                    new NumberingFormat()                                                   // Custom number format # 165: especially for date times (with a blank time)
                    {
                        NumberFormatId = UInt32Value.FromUInt32(iExcelIndex++),
                        FormatCode = StringValue.FromString("dd/MMM/yyyy")
                    }
               ),
                new Fonts(
                    new Font(                                                               // Index 0 - The default font.
                        new FontSize() { Val = 10 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Arial" }),
                    new Font(                                                               // Index 1 - A 12px bold font, in white.
                        new Bold(),
                        new FontSize() { Val = 12 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "FFFFFF" } },
                        new FontName() { Val = "Arial" }),
                    new Font(                                                               // Index 2 - An Italic font.
                        new Italic(),
                        new FontSize() { Val = 10 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Times New Roman" })
                ),
                new Fills(
                    new Fill(                                                           // Index 0 - The default fill.
                        new PatternFill() { PatternType = PatternValues.None }),
                    new Fill(                                                           // Index 1 - The default fill of gray 125 (required)
                        new PatternFill() { PatternType = PatternValues.Gray125 }),
                    new Fill(                                                           // Index 2 - The yellow fill.
                        new PatternFill(
                            new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "FFFFFF00" } }
                        )
                        { PatternType = PatternValues.Solid }),
                    new Fill(                                                           // Index 3 - Dark-gray fill.
                        new PatternFill(
                            new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "FF404040" } }
                        )
                        { PatternType = PatternValues.Solid })
                ),
                new Borders(
                    new Border(                                                         // Index 0 - The default border.
                        new LeftBorder(),
                        new RightBorder(),
                        new TopBorder(),
                        new BottomBorder(),
                        new DiagonalBorder()),
                    new Border(                                                         // Index 1 - Applies a Left, Right, Top, Bottom border to a cell
                        new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder())
                ),
                new CellFormats(
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 },                         // Style # 0 - The default cell style.  If a cell does not have a style index applied it will use this style combination instead
                    new CellFormat() { NumberFormatId = 164 },                                         // Style # 1 - DateTimes
                    new CellFormat() { NumberFormatId = 165 },                                         // Style # 2 - Dates (with a blank time)
                    new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center })
                    { FontId = 1, FillId = 3, BorderId = 0, ApplyFont = true, ApplyAlignment = true },       // Style # 3 - Header row 
                    new CellFormat() { NumberFormatId = 3 },                                           // Style # 4 - Number format: #,##0
                    new CellFormat() { NumberFormatId = 4 },                                           // Style # 5 - Number format: #,##0.00
                    new CellFormat() { FontId = 1, FillId = 0, BorderId = 0, ApplyFont = true },       // Style # 6 - Bold 
                    new CellFormat() { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true },       // Style # 7 - Italic
                    new CellFormat() { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true },       // Style # 8 - Times Roman
                    new CellFormat() { FontId = 0, FillId = 2, BorderId = 0, ApplyFill = true },       // Style # 9 - Yellow Fill
                    new CellFormat(                                                                    // Style # 10 - Alignment
                        new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }
                    )
                    { FontId = 0, FillId = 0, BorderId = 0, ApplyAlignment = true },
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true }      // Style # 11 - Border
                )
            ); // return
        }

        private static void AppendHeaderTextCell(string cellReference, string cellStringValue, OpenXmlWriter writer)
        {
            //  Add a new "text" Cell to the first row in our Excel worksheet
            //  We set these cells to use "Style # 3", so they have a gray background color & white text.
            writer.WriteElement(new Cell
            {
                CellValue = new CellValue(cellStringValue),
                CellReference = cellReference,
                DataType = CellValues.String,
                StyleIndex = 3
            });
        }

        private static string ReplaceHexadecimalSymbols(string txt)
        {
            //  I've often seen cases when a non-ASCII character will slip into the data you're trying to export, and this will cause an invalid Excel to be created.
            //  This function removes such characters.
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }

        private static void AppendNumericCell(string cellReference, string cellStringValue, bool bIncludeDecimalPlaces, OpenXmlWriter writer)
        {
            //  Add a new numeric Excel Cell to our Row.
            UInt32 cellStyle = (UInt32)(bIncludeDecimalPlaces ? 5 : 4);
            writer.WriteElement(new Cell
            {
                CellValue = new CellValue(cellStringValue),
                CellReference = cellReference,
                StyleIndex = cellStyle,                                 //  Style #4 formats with 0 decimal places, style #5 formats with 2 decimal places
                DataType = CellValues.Number
            });
        }

        private static void AppendDateCell(string cellReference, DateTime dateTimeValue, OpenXmlWriter writer)
        {
            //  Add a new "datetime" Excel Cell to our Row.
            //
            //  If the "time" part of the DateTime is blank, we'll format the cells as "dd/MMM/yyyy", otherwise ""dd/MMM/yyyy hh:mm:ss"
            //  (Feel free to modify this logic if you wish.)
            //
            //  In our GenerateStyleSheet() function, we defined two custom styles, to make this work:
            //      Custom style#1 is a style containing our custom NumberingFormat # 164 (show each date as "dd/MMM/yyyy hh:mm:ss")
            //      Custom style#2 is a style containing our custom NumberingFormat # 165 (show each date as "dd/MMM/yyyy")
            //  
            //  So, if our time element is blank, we'll assign style 2, but if there IS a time part, we'll apply style 1.
            //
            string cellStringValue = dateTimeValue.ToOADate().ToString(CultureInfo.InvariantCulture);
            bool bHasBlankTime = (dateTimeValue.Date == dateTimeValue);

            writer.WriteElement(new Cell
            {
                CellValue = new CellValue(cellStringValue),
                CellReference = cellReference,
                StyleIndex = UInt32Value.FromUInt32(bHasBlankTime ? (uint)2 : (uint)1),
                DataType = CellValues.Number        //  Use this, rather than CellValues.Date
            });
        }

        private static void AppendTextCell(string cellReference, string cellStringValue, OpenXmlWriter writer)
        {
            //  Add a new "text" Cell to our Row 

#if DATA_CONTAINS_FORMULAE
            //  If this item of data looks like a formula, let's store it in the Excel file as a formula rather than a string.
            if (cellStringValue.StartsWith("="))
            {
                AppendFormulaCell(cellReference, cellStringValue, writer);
                return;
            }
#endif

            //  Add a new Excel Cell to our Row 
            writer.WriteElement(new Cell
            {
                CellValue = new CellValue(cellStringValue),
                CellReference = cellReference,
                DataType = CellValues.String
            });
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                // Console.WriteLine("This is the first call to Dispose. Necessary clean-up will be done!");

                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    // Console.WriteLine("Explicit call: Dispose is called by the user.");
                }
                else
                {
                    // Console.WriteLine("Implicit call: Dispose is called through finalization.");
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Console.WriteLine("Unmanaged resources are cleaned up here.");

                // TODO: set large fields to null.

                disposedValue = true;
            }
            else
            {
                // Console.WriteLine("Dispose is called more than one time. No need to clean up!");
            }
        }



        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

       
        #endregion
    }


    public class ListtoDataTableConverter
    {

        public DataTable ToDataTable<T>(List<T> items)
        {

            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties

            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in Props)

            {

                //Setting column names as Property names

                dataTable.Columns.Add(prop.Name);

            }

            foreach (T item in items)

            {

                var values = new object[Props.Length];

                for (int i = 0; i < Props.Length; i++)

                {

                    //inserting property values to datatable rows

                    values[i] = Props[i].GetValue(item, null);

                }

                dataTable.Rows.Add(values);

            }

            //put a breakpoint here and check datatable

            return dataTable;

        }

    }
}
