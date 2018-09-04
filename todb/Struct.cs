using Microsoft.Xrm.Sdk;
using System;

namespace todb
{
    public class Table
    {
        public String Name { get; set; }
        public Field[] Fields { get; set; }
        public ID Id { get; set; }
    }

    public class Field
    {
        public String Caption { get; set; }
        public String Value { get; set; }
        public String Format { get; set; }
        public String RefTable { get; set; }
        public Guid RefId { get; set; }
    }

    public class ID
    {
        public String Name { get; set; }
        public Guid Value { get; set; }
    }

    public class Request
    {
        public String Exception { get; set; }
    }

    public class ReadRequest : Request
    {
        public EntityCollection Entity { get; set; }
    }

    public class CreateRequest : Request
    {
        public String Create { get; set; } 
        public Guid Id { get; set; }
    }

    public class UpdateRequest : Request
    {
        public Update[] Update { get; set; }
    }

    public class DeleteRequest : Request
    {
        public String Delete { get; set; }
    }

    public class Update
    {
        public String Caption { get; set; }
        public String Format { get; set; }
        public String Answer { get; set; }
    }
}