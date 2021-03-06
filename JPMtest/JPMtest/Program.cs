﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JPMtest
{
    class Program
    {
        static List<Sale> storedSales = new List<Sale>();

        static void Main(string[] args)
        {   
            string path = @"TestData.txt";
            string line;
            int messagesCounter = 0;

            Console.WriteLine("Message format: <1>,<2>,<3>,<4>,<5>,<6>");
            Console.WriteLine("<1> - Message Type: 1,2,3");
            Console.WriteLine("<2> - Product: 1-apple,2-orange,3-pear");
            Console.WriteLine("<3> - Cost, pence: ");
            Console.WriteLine("<4> - Amount, items: ");
            Console.WriteLine("<5> - Action, +/-/* : ");
            Console.WriteLine("<6> - Amendment, pence: ");
            Console.WriteLine("Loading messages from file: /Debug/TestData.txt");
            
            using (var streamReader = new StreamReader(path, Encoding.UTF8))
            {               
                
                while ((line = streamReader.ReadLine()) != null)
                {
                    if(line == "")
                    {
                        continue;
                    }

                    Console.WriteLine("");
                    Console.WriteLine(String.Format("Mes N{0}: {1}", messagesCounter, line));

                    Sale sale = ParseMessage(line);

                    PrintSale(sale);                   

                    switch (sale.SaleMessageType)
                    {
                        case MessageType.Mes1:
                            storedSales.Add(sale);
                            Console.WriteLine("Mes1 processed.");                           
                            break;
                        case MessageType.Mes2:
                            AddSaleDetails(sale);
                            Console.WriteLine("Mes2 processed.");
                            break;
                        case MessageType.Mes3:
                            UpdateSaleDetails(sale);
                            Console.WriteLine("Mes3 processed.");
                            break;
                        default:
                            
                            break;
                    }

                    messagesCounter++;

                    if((messagesCounter % 5) == 0)
                    {
                        PrintReport10();
                    }

                    if ((messagesCounter % 10) == 0)
                    {
                        PrintReport50();

                        Console.WriteLine("Press a key to continue");
                        Console.ReadKey();
                    }
                }

                PrintReport10();
                PrintReport50();
                Console.WriteLine("");
                Console.WriteLine(String.Format("Total processed messages = {0}", messagesCounter));
            }

            Console.WriteLine("Press a key to finish");
            Console.ReadKey();
            
        }

        static Sale ParseMessage(string line)
        {
            Sale sale = null;

            string[] words = line.Split(',');

            if(words.Count() == 6)
            {
                sale = new Sale();

                int mt; 
                int.TryParse(words[0], out mt);                
                sale.SaleMessageType = (MessageType)mt;

                int product;
                int.TryParse(words[1], out product);
                sale.Product = (ProductType)product;

                int price;
                int.TryParse(words[2], out price);
                sale.Price = price;

                int amount;
                int.TryParse(words[3], out amount);
                sale.Amount = amount;

                string action = words[4].Trim();
                switch(action)
                {
                    case "-":
                        sale.Action = ActionType.Subtruct;
                        break;
                    case "+":
                        sale.Action = ActionType.Add;
                        break;
                    case "*":
                        sale.Action = ActionType.Multiply;
                        break;
                    default:
                        sale.Action = ActionType.None;
                        //log
                        break;
                }
                                
                int adjustment;
                int.TryParse(words[5], out adjustment);
                sale.Adjustment = adjustment;
                
            }
            else
            {
                //log
            }

            return sale;
        }

        static bool AddSaleDetails(Sale sale)
        {
            bool result = true;

            var detailsList = storedSales.Where(s => s.SaleMessageType == MessageType.Mes1 &&
                                            s.Product == sale.Product &&
                                            s.Price == sale.Price);

            int count = detailsList.Count();

            if (count > 0)
            {
                foreach (var saleDetail in detailsList)
                {
                    saleDetail.Amount = sale.Amount;
                    saleDetail.SaleMessageType = MessageType.Mes2;
                    PrintSale(saleDetail);
                }
            }
            else
            {
                result = false;
            }
                       
            Console.WriteLine(String.Format("Mes2 details added for {0} sales", count));
           
            return result;
        }

        static bool UpdateSaleDetails(Sale sale)
        {
            bool result = true;

            var adjList = storedSales.Where(s => s.SaleMessageType == MessageType.Mes2 &&
                                            s.Product == sale.Product 
                                            );

            if (adjList.Count() > 0)
            {
                foreach (var saleDetail in adjList)
                {
                    saleDetail.Action = sale.Action;
                    saleDetail.Adjustment = sale.Adjustment;

                    PrintSale(saleDetail);
                }
            }
            else
            {
                result = false;
            }

            Console.WriteLine(String.Format("Mes3 adjusment applied for {0} sales", adjList.Count()));

            return result;
        }
        

        static void PrintSale(Sale sale)
        {
            Console.WriteLine(String.Format("Message Type={0}; Product={1}; Price={2}p; Amount={3}; Action={4}{5}; Cost={6}, Total={7}",
                   sale.SaleMessageType, sale.Product, sale.Price, sale.Amount, sale.Action, sale.Adjustment, sale.Cost, sale.TotalPrice));
        }

        static void PrintReport10()
        {            
            Console.WriteLine("");
            Console.WriteLine("Report for basic sales:");
            Console.WriteLine("======================");

            var saleList = storedSales.Where(s => s.SaleMessageType == MessageType.Mes2 && s.Adjustment == 0);

            foreach (Sale sale in saleList)
            {               
                PrintSale(sale);
            }

            Console.WriteLine("End of Report");            
            Console.WriteLine("");
        }

        static void PrintReport50()
        {
            Console.WriteLine("");
            Console.WriteLine("Report for adjasted sales:");
            Console.WriteLine("=========================");

            var saleList = storedSales.Where(s => s.SaleMessageType == MessageType.Mes2 && s.Adjustment != 0);

            foreach (Sale sale in saleList)
            {
                PrintSale(sale);
            }

            Console.WriteLine("End of Report");
            Console.WriteLine("");
        }
    }

    
}
