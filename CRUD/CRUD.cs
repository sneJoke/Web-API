using System;
using System.Collections.Generic;
using System.ServiceModel;
using CrmServiceHelpers;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace CRUD
{
    public class CRUD
    {
        private Guid _accountId;
        private OrganizationServiceProxy _serviceProxy;
        private String UserName;
        private ServerConnection sCon;
        private ServerConnection.Configuration config;

        private String getSsl(Boolean b)
        {
            return b ? "https" : "http";
        }

        public String Connect(String ServerAddress, String OrganizationName, String Login, String Domain, String Password, Boolean Ssl)
        {
            try
            {
                sCon = new ServerConnection();
                config = sCon.GetServerConfiguration(ServerAddress,
                                                     OrganizationName,
                                                     new Uri(getSsl(Ssl) + "://" + ServerAddress + "/XRMServices/2011/Discovery.svc"),
                                                     new Uri(getSsl(Ssl) + "://" + ServerAddress + "/"+ OrganizationName + "/XRMServices/2011/Organization.svc"),
                                                     "ActiveDirectory",
                                                     Login,
                                                     Domain,
                                                     Password);
                UserName = RunUser(Login, Domain);
                return "Connected";
            }
            catch (System.ServiceModel.Security.SecurityNegotiationException ex)
            {
                return "Введен неверный логин/пароль. " + ex.Message;
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                return "Приложение завершилось с ошибкой: " + ex.Detail.Message;
            }
            catch (System.TimeoutException ex)
            {
                return "Превышен лимит ожидания. " + ex.Message;
            }
            catch (System.Exception ex)
            {
                return "Приложение завершилось с системной ошибкой: " + ex.Message + " " + ex.InnerException.Message;
            }
        }

        public String CreateRowEntity(String entityName)
        {
            try
            {
                Entity entity = new Entity(entityName);
                _accountId = _serviceProxy.Create(entity);
                return "Created";
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                return "Запись не создалась. " + ex.Message;
            }
        }

        public String CreateRow(Entity entity)
        {
            try
            {
                _accountId = _serviceProxy.Create(entity);
                return "Created";
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                return "Запись не создалась. " + ex.Message;
            }
        }

        public Guid getAccount()
        {
            return _accountId;
        }

        public String UpdateRow(String entityName, String column, String value, Guid guid)
        {
            try
            {
                ColumnSet cols = new ColumnSet(column);
                Entity entity = _serviceProxy.Retrieve(entityName, guid, cols);
                entity[column] = value;
                _serviceProxy.Update(entity);
                return "Updated";
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                return "Поле '" + column + "' не обновилось. " + ex.Message;
            }
        }

        public String UpdateRow(String entityName, String column, Decimal value, Guid guid)
        {
            try
            {
                ColumnSet cols = new ColumnSet(column);
                Entity entity = _serviceProxy.Retrieve(entityName, guid, cols);
                entity[column] = value;
                _serviceProxy.Update(entity);
                return "Updated";
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                return "Поле '" + column + "' не обновилось. " + ex.Message;
            }
        }

        public String UpdateRow(String entityName, String column, OptionSetValue value, Guid guid)
        {
            try
            {
                ColumnSet cols = new ColumnSet(column);
                Entity entity = _serviceProxy.Retrieve(entityName, guid, cols);
                entity[column] = value;
                _serviceProxy.Update(entity);
                return "Updated";
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                return "Поле '" + column + "' не обновилось. " + ex.Message;
            }
        }

        public String UpdateRow(String entityName, String column, int value, Guid guid)
        {
            try
            {
                ColumnSet cols = new ColumnSet(column);
                Entity entity = _serviceProxy.Retrieve(entityName, guid, cols);
                entity[column] = value;
                _serviceProxy.Update(entity);
                return "Updated";
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                return "Поле '" + column + "' не обновилось. " + ex.Message;
            }
        }

        public String UpdateRow(String entityName, String column, Double value, Guid guid)
        {
            try
            {
                ColumnSet cols = new ColumnSet(column);
                Entity entity = _serviceProxy.Retrieve(entityName, guid, cols);
                entity[column] = value;
                _serviceProxy.Update(entity);
                return "Updated";
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                return "Поле '" + column + "' не обновилось. " + ex.Message;
            }
        }

        public String UpdateRow(String entityName, String column, Boolean value, Guid guid)
        {
            try
            {
                ColumnSet cols = new ColumnSet(column);
                Entity entity = _serviceProxy.Retrieve(entityName, guid, cols);
                entity[column] = value;
                _serviceProxy.Update(entity);
                return "Updated";
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                return "Поле '" + column + "' не обновилось. " + ex.Message;
            }
        }

        public String UpdateRow(String entityName, String column, DateTime value, Guid guid)
        {
            try
            {
                ColumnSet cols = new ColumnSet(column);
                Entity entity = _serviceProxy.Retrieve(entityName, guid, cols);
                entity[column] = value;
                _serviceProxy.Update(entity);
                return "Updated";
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                return "Поле '" + column + "' не обновилось. " + ex.Message;
            }
        }

        public String UpdateRow(String entityName, String column, EntityReference entityReference, Guid guid)
        {
            try
            {
                ColumnSet cols = new ColumnSet(column);
                Entity entity = _serviceProxy.Retrieve(entityName, guid, cols);
                entity[column] = entityReference;
                _serviceProxy.Update(entity);
                return "Updated";
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                return "Поле '" + column + "' не обновилось. " + ex.Message;
            }
        }

        public String DeleteRow(String entityName, Guid Id)
        {
            try
            {
                int activityState = 1;
                int activityStatus = 2;

                SetStateRequest activityReq = new SetStateRequest();
                activityReq.EntityMoniker = new EntityReference(entityName, Id);
                activityReq.State = new OptionSetValue(activityState);
                activityReq.Status = new OptionSetValue(activityStatus);

                SetStateResponse response = (SetStateResponse)_serviceProxy.Execute(activityReq);
                return "Deleted";
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                return "Удаление не произошло. " + ex.Message;
            }
        }

        private String RunUser(String strUser, String strDomain)
        {
            String s = String.Empty;
            String fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                            <entity name='systemuser'>
                            <attribute name='fullname' />
                            <order attribute='fullname' descending='false' />
                            <filter type='or'>
                            <condition attribute='domainname' operator='eq' value='" + strUser + @"@" + strDomain + @"' />
                            <condition attribute='domainname' operator='eq' value='" + strDomain + @"\" + strUser + @"' />
                            </filter>
                            </entity>
                            </fetch>";
            try
            {
                _serviceProxy = ServerConnection.GetOrganizationProxy(config);
                EntityCollection result = _serviceProxy.RetrieveMultiple(new FetchExpression(fetch));
                foreach (var c in result.Entities)
                {
                    s = c.Attributes["fullname"].ToString();
                }
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
            {
            }
            return s;
        }

        public EntityCollection RunFetch(string EntityName, Guid IdValue, string IdName, string Fields)
        {
            EntityCollection result = new EntityCollection();
            String fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                            <entity name='"+ EntityName + @"'>
                            "+ Fields + @"
                            <filter type='and'>
                            <condition attribute='"+IdName+ @"' operator='eq' value='" + IdValue + @"' />
                            </filter>
                            </entity>
                            </fetch>";
            try
            {
                _serviceProxy = ServerConnection.GetOrganizationProxy(config);
                result = _serviceProxy.RetrieveMultiple(new FetchExpression(fetch));
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
            {
            }
            return result;
        }

        public EntityCollection RunFetch(string EntityName)
        {
            EntityCollection result = new EntityCollection();
            String fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                            <entity name='" + EntityName + @"'>
                            <attribute name = '" + EntityName + @"id' /> 
                            </entity>
                            </fetch>";
            try
            {
                _serviceProxy = ServerConnection.GetOrganizationProxy(config);
                result = _serviceProxy.RetrieveMultiple(new FetchExpression(fetch));
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
            {
            }
            return result;
        }

        public void Disconnect()
        {
            sCon = null;
            config = null;
            UserName = null;
            _accountId = Guid.Empty;
            _serviceProxy = null;
        }
    }
}
