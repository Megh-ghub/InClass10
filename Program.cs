using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InClass10
{
    //Entity for Product
    public class Product
    {
        public int ID { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public ICollection<ProductOrder> productorders { get; set; }
    }
    //Entity for Order
    public class Order
    {
        public int ID { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public ICollection<ProductOrder> orderedproducts { get; set; }
    }

    //Associate entity for ProductOrder
    public class ProductOrder
    {
        public int ID { get; set; }
        public Order order { get; set; }
        public Product product { get; set; }
        public int Quantity { get; set; }
    }

    class ProductOrderContext : DbContext
    {
        public DbSet<Product> products { get; set; }
        public DbSet<Order> orders { get; set; }
        public DbSet<ProductOrder> productorders { get; set; }

        string connectionString = "Server=(localdb)\\mssqllocaldb;Database=InClass10;Trusted_Connection=True;MultipleActiveResultSets=true";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<int>("Product_seq", schema: "dbo")
                .StartsAt(100)
                .IncrementsBy(1);

            modelBuilder.Entity<Product>()
                .Property(p => p.ID)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.Product_seq");

            modelBuilder.HasSequence<int>("Order_seq", schema: "dbo")
                .StartsAt(1000)
                .IncrementsBy(1);

            modelBuilder.Entity<Order>()
                .Property(o => o.ID)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.Order_seq");

            modelBuilder.HasSequence<int>("ProductOrder_seq", schema: "dbo")
                .StartsAt(10000)
                .IncrementsBy(1);

            modelBuilder.Entity<ProductOrder>()
                .Property(po => po.ID)
                .HasDefaultValueSql("NEXT VALUE FOR dbo.ProductOrder_seq");

        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new ProductOrderContext())
            {
                context.Database.EnsureCreated();

                Product[] productlist = new Product[]
                {
                    new Product() {ProductName="Baskin Robins",Price=100.00},
                    new Product() {ProductName="Kwality Magnum",Price=10.00},
                    new Product() {ProductName="Chobani Yogurt",Price=12.00},
                    new Product() {ProductName="Mayonnaise",Price=4.00},
                    new Product() {ProductName="Coca Cola",Price=3.00},
                    new Product() {ProductName="Butter",Price=5.00}
                };

                if (!context.products.Any())
                {
                    foreach (Product p in productlist)
                    {
                        context.products.Add(p);
                    }
                    context.SaveChanges();
                }

                Order[] orderlist = new Order[]
                {
                    new Order() {OrderDate=DateTime.Now,CustomerName="Gloria Ghosh"},
                    new Order() {OrderDate=DateTime.Now,CustomerName="David Friedenberg"},
                    new Order() {OrderDate=DateTime.Now,CustomerName="John Hillyer"},
                    new Order() {OrderDate=DateTime.Now,CustomerName="Bill Gates"},
                    new Order() {OrderDate=DateTime.Now,CustomerName="Don Bosko"},
                    new Order() {OrderDate=DateTime.Now,CustomerName="Bob Nines"}
                };

                if (!context.orders.Any())
                {
                    foreach (Order o in orderlist)
                    {
                        context.orders.Add(o);
                    }
                    context.SaveChanges();
                }

                ProductOrder[] productorderlist = new ProductOrder[]
                {
                    new ProductOrder() {order=orderlist[0],product=productlist[0],Quantity=2},
                    new ProductOrder() {order=orderlist[0],product=productlist[1],Quantity=1},
                    new ProductOrder() {order=orderlist[0],product=productlist[2],Quantity=5},
                    new ProductOrder() {order=orderlist[1],product=productlist[3],Quantity=2},
                    new ProductOrder() {order=orderlist[1],product=productlist[4],Quantity=1},
                    new ProductOrder() {order=orderlist[2],product=productlist[4],Quantity=4},
                    new ProductOrder() {order=orderlist[2],product=productlist[2],Quantity=1},
                    new ProductOrder() {order=orderlist[3],product=productlist[3],Quantity=5},
                    new ProductOrder() {order=orderlist[3],product=productlist[2],Quantity=1},
                    new ProductOrder() {order=orderlist[3],product=productlist[1],Quantity=4},
                    new ProductOrder() {order=orderlist[3],product=productlist[4],Quantity=3},
                    new ProductOrder() {order=orderlist[4],product=productlist[4],Quantity=4},
                    new ProductOrder() {order=orderlist[4],product=productlist[0],Quantity=5},
                };

                if (!context.productorders.Any())
                {
                    foreach (ProductOrder po in productorderlist)
                    {
                        context.productorders.Add(po);
                    }
                    context.SaveChanges();
                }

                // Display all orders where a product is sold
                //Order 6 from Customer'Bob Nines' do not have any product , so not in display. 
                var a = context.orders
                    .Include(c => c.orderedproducts)
                    .Where(c => c.orderedproducts.Count != 0);
                Console.WriteLine("***Order where a product is sold***");
                foreach (var i in a)
                {
                    Console.WriteLine("OrderID={0},OrderDate={1},CustomerName={2}", i.ID, i.OrderDate, i.CustomerName);
                }

                // For a given product, find the order where it is sold the maximum.
                // Butter was never ordered so typing Butter returns Exception
                Console.WriteLine("\nEnter Productname for Maximum Order : ");
                Console.WriteLine("\n | Baskin Robins | Kwality Magnum | Chobani Yogurt | Mayonnaise | Coca Cola | Butter |");
                string input = Console.ReadLine();

                Order output = context.productorders
                    .Where(c => c.product.ProductName == input)
                    .OrderByDescending(c => c.Quantity)
                    .Select(c => c.order)
                    .FirstOrDefault();
                Console.WriteLine("---------------------Max {0} Order---------------------", input);
                Console.WriteLine("OrderID={0},OrderDate={1},CustomerName={2}", output.ID, output.OrderDate, output.CustomerName);
            }
        }
    }
}