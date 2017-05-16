using System;
using Nancy;

namespace HelloMicroservices
{
    public class CurrentDateTimeModule : NancyModule
    {
        public CurrentDateTimeModule()
        {
            Get("/", _ => DateTime.Now);
        }
    }
}