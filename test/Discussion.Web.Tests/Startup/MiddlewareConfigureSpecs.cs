﻿using Microsoft.AspNet.Builder.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.Http.Internal;
using Xunit;
using Microsoft.AspNet.Http;
using Moq;
using Microsoft.AspNet.Hosting;

namespace Discussion.Web.Tests.Startup
{
    public class MiddlewareConfigureSpecs
    {
        [Fact]
        public void should_add_iis_platform()
        {
            // arrange
            var httpContext = GivenHttpContextFromIISPlatformHandler();
            var startup = new Web.Startup();
            var services = new ServiceCollection().BuildServiceProvider();
            var app = new ApplicationBuilder(services);

            // act
            startup.Configure(app, (new Mock<IHostingEnvironment>()).Object);
            app.Build().Invoke(httpContext);

            // assert
            AssertHttpContextAreProperlyRestored(httpContext);
        }




        const string Req_IP = "128.0.0.1";
        const string XForwardedForHeaderName = "X-Forwarded-For";

        const string Req_ProtoValue = "FTP";
        const string XForwardedProtoHeaderName = "X-Forwarded-Proto";

        const string Req_ProtoAfterModified = "FTP-OVER-HTTP";        
        const string XOriginalProtoName = "X-Original-Proto";


        HttpContext GivenHttpContextFromIISPlatformHandler()
        {
            // IISPlatformHandlerMiddleware is used to restore the Headers marked(modified) by the IISPlatformHandler module
            // see https://github.com/aspnet/IISIntegration/blob/2fe2e0d8418ff55612fc9001b7f7bde058ae5bb9/src/Microsoft.AspNet.IISPlatformHandler/IISPlatformHandlerMiddleware.cs

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add(XForwardedProtoHeaderName, Req_ProtoValue);
            httpContext.Request.Headers.Add(XForwardedForHeaderName, Req_IP);

            httpContext.Request.Scheme = Req_ProtoAfterModified;

            return httpContext;
        }

        void AssertHttpContextAreProperlyRestored(HttpContext httpContext)
        {
            httpContext.Request.Scheme.ShouldEqual(Req_ProtoValue);
            // httpContext.Request.Headers.ShouldContain(kvp => kvp.Key == XOriginalProtoName && kvp.Value == Req_ProtoAfterModified);

            httpContext.Connection.RemoteIpAddress.ToString().ShouldEqual(Req_IP);
        }
    }
}