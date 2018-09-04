using System;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using Microsoft.Xrm.Sdk;

namespace todb
{
    [ScriptService]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [WebService(Description = "CRUD", Namespace = "Aston")]
    public class Service : WebService
    {
        #region Обявление переменных
        /*Объявление объекта для подключения и CRUD операций*/
        private CRUD.CRUD Connection = new CRUD.CRUD();
        private JavaScriptSerializer js = new JavaScriptSerializer();
        #endregion

        #region Конструктор с логином/паролем
        public Service()
        {
            /*Подключение и обнуление ответа*/
            Connection.Connect("crm", "orgname", "login", "domain", "pwd", true);
        }
        #endregion

        #region Методы
        /*Возвращаем данные по GUID*/
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod(Description = "Возвращаем данные по GUID")]
        public string ReadData(Table table)
        {
            ReadRequest req = new ReadRequest();
            try
            {
                /*Перечисляем поля для Fetch запроса*/
                String fields = String.Empty;
                foreach (Field field in table.Fields)
                {
                    fields = fields + "<attribute name='" + field.Caption + "' />";
                }

                /*Формируем ответ на запрос*/
                req.Entity = Connection.RunFetch(table.Name, table.Id.Value, table.Id.Name, fields);
            }
            /*Обработка ошибок*/
            catch (Exception e)
            {
                req.Exception = e.Message;
            }
            Connection.Disconnect();
            return js.Serialize(req);
        }

        /*Созадем запись в таблице*/
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod(Description = "Созадем запись в таблице")]
        public string CreateData(Table table)
        {
            CreateRequest req = new CreateRequest();
            try
            {
                Entity entity = new Entity(table.Name);
                foreach (Field field in table.Fields)
                {
                    switch (field.Format)
                    {
                        case "string":
                            entity[field.Caption] = field.Value;
                            break;
                        case "boolean":
                            entity[field.Caption] = Boolean.Parse(field.Value);
                            break;
                        case "double":
                            entity[field.Caption] = Double.Parse(field.Value);
                            break;
                        case "decimal":
                            entity[field.Caption] = Decimal.Parse(field.Value);
                            break;
                        case "int":
                            entity[field.Caption] = int.Parse(field.Value);
                            break;
                        case "vps":
                            entity[field.Caption] = new OptionSetValue(int.Parse(field.Value));
                            break;
                        case "datetime":
                            entity[field.Caption] = DateTime.Parse(field.Value);
                            break;
                        case "reference":
                            entity[field.Caption] = new EntityReference(field.RefTable, field.RefId);
                            break;
                    }
                }
                /*Формируем ответ на запрос*/
                req.Create = Connection.CreateRow(entity);
                req.Id = Connection.getAccount();
            }
            catch (Exception e)
            {
                req.Exception = e.Message;
            }
            Connection.Disconnect();
            return js.Serialize(req);
        }

        /*Обновляем строку*/
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod(Description = "Обновляем строку")]
        public string UpdateData(Table table)
        {
            UpdateRequest req = new UpdateRequest();
            try
            {
                Update[] update = new Update[table.Fields.Length];
                String message = String.Empty;
                int count = 0;

                foreach (Field field in table.Fields)
                {
                    switch (field.Format)
                    {
                        case "string":
                            message = Connection.UpdateRow(table.Name, field.Caption, field.Value, table.Id.Value);
                            break;
                        case "boolean":
                            message = Connection.UpdateRow(table.Name, field.Caption, Boolean.Parse(field.Value), table.Id.Value);
                            break;
                        case "int":
                            message = Connection.UpdateRow(table.Name, field.Caption, int.Parse(field.Value), table.Id.Value);
                            break;
                        case "vps":
                            message = Connection.UpdateRow(table.Name, field.Caption, new OptionSetValue(int.Parse(field.Value)), table.Id.Value);
                            break;
                        case "double":
                            message = Connection.UpdateRow(table.Name, field.Caption, Double.Parse(field.Value), table.Id.Value);
                            break;
                        case "decimal":
                            message = Connection.UpdateRow(table.Name, field.Caption, Decimal.Parse(field.Value), table.Id.Value);
                            break;
                        case "datetime":
                            message = Connection.UpdateRow(table.Name, field.Caption, DateTime.Parse(field.Value), table.Id.Value);
                            break;
                        case "reference":
                            message = Connection.UpdateRow(table.Name, field.Caption, new EntityReference(field.RefTable, field.RefId), table.Id.Value);
                            break;
                        default:
                            message = "Unknown type";
                            break;
                    }
                    update[count] = new Update();
                    update[count].Caption = field.Caption;
                    update[count].Format = field.Format;
                    update[count++].Answer = message;
                }
                /*Формируем ответ на запрос*/
                req.Update = update;
            }
            catch (Exception e)
            {
                req.Exception = e.Message;
            }
            Connection.Disconnect();
            return js.Serialize(req);
        }

        /*Поменить строку неактивной*/
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod(Description = "Поменить строку неактивной")]
        public string DeleteData(Table table)
        {
            DeleteRequest req = new DeleteRequest();
            try
            {
                req.Delete = Connection.DeleteRow(table.Name, table.Id.Value);
            }
            catch (Exception e)
            {
                req.Exception = e.Message;
            }
            Connection.Disconnect();
            return js.Serialize(req);
        }

        /*Поменить строку неактивной*/
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod(Description = "Все id")]
        public string SelectData(Table table)
        {
            ReadRequest req = new ReadRequest();
            try
            {
                req.Entity = Connection.RunFetch(table.Name);
            }
            catch (Exception e)
            {
                req.Exception = e.Message;
            }
            Connection.Disconnect();
            return js.Serialize(req);
        }
        #endregion
    }
}