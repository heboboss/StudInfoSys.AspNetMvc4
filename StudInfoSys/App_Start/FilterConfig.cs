﻿using StudInfoSys.Helpers;
using System.Web.Mvc;

namespace StudInfoSys.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new MyHandleErrorAttribute());
        }
    }
}