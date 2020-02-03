﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Northwind.CurrencyServices.CountryCurrency;
using Northwind.CurrencyServices.CurrencyExchange;
using Northwind.ReportingServices.OData.ProductReports;

namespace ReportingApp
{
    /// <summary>
    /// Program class.
    /// </summary>
    public sealed class Program
    {
        private const string NorthwindServiceUrl = "https://services.odata.org/V3/Northwind/Northwind.svc";
        private const string CurrentProductsReport = "current-products";
        private const string MostExpensiveProductsReport = "most-expensive-products";

        /// <summary>
        /// A program entry point.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                ShowHelp();
                return;
            }

            var reportName = args[0];

            if (string.Equals(reportName, CurrentProductsReport, StringComparison.InvariantCultureIgnoreCase))
            {
                var service = new ProductReportService(new Uri(NorthwindServiceUrl));

                var countryService = new Northwind.CurrencyServices.CountryCurrency.CountryCurrencyService();
                /*
                                var countries = await service.Get();

                                Dictionary<string, CountryInfo> dict = new Dictionary<string, CountryInfo>();

                                foreach (var country in countries)
                                {
                                    var countryInfo = await countryService.Lookup(country);
                                    dict.Add(country, countryInfo);
                                }
                */
                //await countryService.GetCurrencyInfo();

                await ShowCurrentProducts();
                return;
            }
            else if (string.Equals(reportName, MostExpensiveProductsReport, StringComparison.InvariantCultureIgnoreCase))
            {
                if (args.Length > 1 && int.TryParse(args[1], out int count))
                {
                    await ShowMostExpensiveProducts(count);
                    return;
                }
            }
            else
            {
                ShowHelp();
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\tReportingApp.exe <report> <report-argument1> <report-argument2> ...");
            Console.WriteLine();
            Console.WriteLine("Reports:");
            Console.WriteLine($"\t{CurrentProductsReport}\t\tShows current products.");
            Console.WriteLine($"\t{MostExpensiveProductsReport}\t\tShows specified number of the most expensive products.");
        }

        private static async Task ShowCurrentProducts()
        {
            var service = new ProductReportService(new Uri(NorthwindServiceUrl));
            var countryCurrencyService = new CountryCurrencyService();
            var currencyExchangeService = new CurrencyExchangeService("cd4c05ecaa6787738809c8d290c5acc5");

            var report = await service.GetCurrentProductsWithLocalCurrencyReport(countryCurrencyService, currencyExchangeService);
            PrintProductReport("current products:", report);
        }

        private static async Task ShowMostExpensiveProducts(int count)
        {
            var service = new ProductReportService(new Uri(NorthwindServiceUrl));
            var report = await service.GetMostExpensiveProductsReport(count);
            PrintProductReport($"{count} most expensive products:", report);
        }

        private static void PrintProductReport(string header, ProductReport<ProductPrice> productReport)
        {
            Console.WriteLine($"Report - {header}");
            foreach (var reportLine in productReport.Products)
            {
                Console.WriteLine("{0}, {1}", reportLine.Name, reportLine.Price);
            }
        }

        private static void PrintProductReport(string header, ProductReport<ProductLocalPrice> productReport)
        {
            Console.WriteLine($"Report - {header}");
            foreach (var reportLine in productReport.Products)
            {
                Console.WriteLine("{0}, {1:00}$, {2}, {3:00}{4}", reportLine.Name, reportLine.Price, reportLine.Country, reportLine.LocalPrice, reportLine.CurrencySymbol);
            }
        }
    }
}
