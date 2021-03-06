using System;
using System.Collections.Generic;
using System.Linq;
using bangazon_cli.Models;
using Microsoft.Data.Sqlite;


namespace bangazon_cli.Managers
{
    /*
        Author: Greg Lawrnece
        Responsibility: Manage the database related tasks for Orders
     */
    public class OrderManager
    {
        
        // collection to store orders until our database is setup
        private List<Order> _orders = new List<Order>();
        private DatabaseInterface _db;
        
        
        // injecting databaseinterface dependency
        public OrderManager(DatabaseInterface db)
        {
            _db = db;
            this.CreateOrderTable();
            this.CreateOrderProductTable();
        }

        // created the Order table if it doesn't exist
        private void CreateOrderTable() {
            try {
                _db.Update(@"CREATE TABLE IF NOT EXISTS `Order` (
                    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
                    `CustomerId` INTEGER NOT NULL,
                    `PaymentTypeId` INTEGER,
                    `CompletedDate` TEXT,
                    FOREIGN KEY(`CustomerId`) REFERENCES `Customer`(`Id`)
                    );
                ");
            } catch (Exception ex) {
                Console.WriteLine("CreateOrderTable", ex.Message);
            }
        }

        // creates the OrderProduct joiner table if it doesn't exist
        private void CreateOrderProductTable() {
            try {
                _db.Update(@"CREATE TABLE IF NOT EXISTS `OrderProduct` (
                    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
                    `OrderId` INTEGER NOT NULL,
                    `ProductId` INTEGER NOT NULL,
                    FOREIGN KEY(`OrderId`) REFERENCES `Order`(`Id`)
                    FOREIGN KEY(`ProductId`) REFERENCES `Product`(`Id`)
                    );
                ");
            } catch (Exception ex) {
                Console.WriteLine("CreateOrderTable", ex.Message);
            }
        }
        /*
            Adds a Order record to the database
            Parameters: Order object
            Returns the Id of the order, that is created by the database
        */        
        public int AddOrder(Order order) {
            string SQLInsert = $@"INSERT INTO `Order`
            VALUES (
                null,
                {order.CustomerId},
                null,
                null
            );";

            int orderId = 0;
            try {
                orderId = _db.Insert(SQLInsert);
                order.Id = orderId;
            } catch (Exception err) {
                Console.WriteLine("Add Order Error", err.Message);
            }
            return orderId;
        }

        /* 
            Function that returns customer's unpaid order from the database
            Parameter: customerId - used to query the database and find the current user's active, unpaid order
            Returns an order object that matches the query.
        */
        public Order GetUnpaidOrder(int customerId) {
            // initialize a new order to hold the return from db
            Order order = new Order(customerId);
            // query the database for a matching order
            _db.Query($@"SELECT * 
                        FROM `Order` o
                        WHERE o.CustomerId = {customerId}
                        AND o.PaymentTypeId IS NULL;
                        ",
                        (SqliteDataReader reader) =>
                        {
                            while (reader.Read())
                            {
                                order.Id = Convert.ToInt32(reader["Id"]);
                                order.CustomerId = Convert.ToInt32(reader["CustomerId"]);
                                order.PaymentTypeId = null;
                                order.CompletedDate = null;
                            }
                        });
            return order;
        }

        /* 
        Function to store a product on an order, by adding a record to the OrderProduct joiner table
        Parameters: orderId, productId
        Parameters are used to create a new record on the OrderProduct table that shows the relationship of the product being placed on the order with that orderId. 
        */
        public void AddProductToOrder(int orderId, int productId)
        {
            // add the product and order relationship to the OrderProduct join table
            string SQLInsert = $@"INSERT INTO OrderProduct
            VALUES (
                null,
                {orderId},
                {productId}
            );";

            try {
                _db.Insert(SQLInsert);
            } catch (Exception err) {
                Console.WriteLine("Add OrderProduct Error", err.Message);
            }
        }

        
        /* 
        Function to check if customer's order contains a product.
        Parameters are: orderId
        OrderId is used to retrieve the order from the database.
        Returns an order containing a list of all the products associated with the order
        */
        public Order GetProductsFromOrder(int orderId)
        {
            // initialize a new order to hold the return from db
            Order CurrentOrder = new Order();

            // query the database for a matching order
            _db.Query($@"SELECT o.Id as OrderId, o.CustomerId as CustomerId, o.PaymentTypeId, o.CompletedDate, op.Id as OrderProductId, p.Id as ProductId, p.Name as ProductName, p.Description ProductDescription, p.Price as ProductPrice, p.Quantity as ProductQuantity
                        FROM `Order` o, OrderProduct op, Product p
                        WHERE o.Id = {orderId}
                        AND o.Id = op.OrderId
						AND op.ProductId = p.Id;
                        ",
                        (SqliteDataReader reader) =>
                        {
                            while (reader.Read())
                            {
                                // assign order details to the order created above
                                CurrentOrder.Id = Convert.ToInt32(reader["OrderId"]);
                                CurrentOrder.CustomerId = Convert.ToInt32(reader["CustomerId"]);
                                CurrentOrder.PaymentTypeId = null;
                                CurrentOrder.CompletedDate = null;

                                // create a new product to hold retrieved product from db
                                Product CurProduct = new Product();
                                // assign product details to the product created above
                                CurProduct.Id = Convert.ToInt32(reader["ProductId"]);
                                CurProduct.Name = Convert.ToString(reader["ProductName"]);
                                CurProduct.Price = Convert.ToDouble(reader["ProductPrice"]);
                                CurProduct.Description = Convert.ToString(reader["ProductDescription"]);
                                CurProduct.Quantity = Convert.ToInt32(reader["ProductQuantity"]);
                                
                                // store product in a list on the order
                                CurrentOrder.Products.Add(CurProduct);
                            }
                        });
            
            return CurrentOrder;
        }


        /*
        Function allows the user to retrieve a single product that is on an order
        Parameters are:
        orderId, productId
        
        Parameters orderId and ProductId are used to find an order in the database that has a matching orderId and also contains a product that matches the productId.
        Returns a matching product
        */
         
        public Product GetSingleProductFromOrder(int orderId, int productId)
        {
            try {
                // initialize a new order to hold the return from db
                Order CurrentOrder = new Order();
                // create a new product to hold retrieved product from db
                Product CurProduct = new Product();    

                // query the database for a matching order
                _db.Query($@"SELECT o.Id as OrderId, o.CustomerId as CustomerId, o.PaymentTypeId, o.CompletedDate, op.Id as OrderProductId, p.Id as ProductId, p.Name as ProductName, p.Description ProductDescription, p.Price as ProductPrice, p.Quantity as ProductQuantity
                            FROM `Order` o, OrderProduct op, Product p
                            WHERE o.Id = {orderId}
                            AND o.Id = op.OrderId
                            AND op.ProductId = p.Id
                            AND p.Id = {productId};
                            ",
                            (SqliteDataReader reader) =>
                            {
                                while (reader.Read())
                                {
                                    // assign order details to the order created above
                                    CurrentOrder.Id = Convert.ToInt32(reader["OrderId"]);
                                    CurrentOrder.CustomerId = Convert.ToInt32(reader["CustomerId"]);
                                    CurrentOrder.PaymentTypeId = null;
                                    CurrentOrder.CompletedDate = null;

                                    // assign product details to the product created above
                                    CurProduct.Id = Convert.ToInt32(reader["ProductId"]);
                                    CurProduct.Name = Convert.ToString(reader["ProductName"]);
                                    CurProduct.Price = Convert.ToDouble(reader["ProductPrice"]);
                                    CurProduct.Description = Convert.ToString(reader["ProductDescription"]);
                                    CurProduct.Quantity = Convert.ToInt32(reader["ProductQuantity"]);
                                }
                            });
                return CurProduct;

            } catch  (Exception err) {
                Console.WriteLine("Get Single Product From Order Error", err.Message);
                return null;
            }
        }


        /*
            Function to allow a customer's order to be closed and paid for by adding a payment type and a current date to the respective fields on the order. 
            Parameters are:
            OrderId, PaymentTypeId
            Returns a boolean value of whether the order was successfully closed by saving the changes to the database. Changes will be adding payment type and completion date
        */
        public bool CloseOrder(int orderId, int paymentTypeId)
        {
            // get the current date to place on the order as a closing date
            // DateTime currentDate = DateTime.UtcNow;

            //update order record in Database by adding paymentTypeId and currentDate
            string SQLUpdate = $@"UPDATE `Order` 
            SET PaymentTypeId = {paymentTypeId},
                CompletedDate = current_timestamp
            WHERE `Id` = {orderId};
            ";

            // initialize a variable to hold whether order was successfully updated
            bool success;
            // try to update database
            try
            {
                _db.Update(SQLUpdate);
                success = true;
            }
            catch (Exception err)
            {
                Console.WriteLine("Close Order Error", err.Message);
                success = false;
            }
            // return whether db update was successful
            return success;
        }

        /*
            Function to find an order by Id and return it
            Parameters: orderId
            Returns a matching order
        */
        public Order GetOrderById(int orderId)
        {
            // initialize an order
            Order RequestedOrder = new Order();

            // query the database for a matching order
            try {
                _db.Query($@"SELECT o.Id as OrderId, o.CustomerId as CustomerId, o.PaymentTypeId as PaymentTypeId, o.CompletedDate as CompletedDate
                    FROM `Order` o
                    WHERE o.Id = {orderId};
                    ",
                    (SqliteDataReader reader) =>
                    {
                        while (reader.Read())
                        {
                            // assign order details to the order created above
                            // check if value is null 
                            RequestedOrder.Id = Convert.ToInt32(reader["OrderId"]);
                            RequestedOrder.CustomerId = Convert.ToInt32(reader["CustomerId"]);
                            if (reader.IsDBNull(2)) {
                                RequestedOrder.PaymentTypeId = null;
                            } else {
                                RequestedOrder.PaymentTypeId = Convert.ToInt32(reader["PaymentTypeId"]);
                            }
                            if (reader.IsDBNull(3)) {
                                RequestedOrder.CompletedDate = null;
                            } else {
                                RequestedOrder.CompletedDate = Convert.ToDateTime(reader["CompletedDate"]);
                            }
                        }
                    });
                    // return the RequestedOrder out of the function
                    return RequestedOrder;

            } catch (Exception err) {
                Console.WriteLine("Get Single Product From Order Error", err.Message);
                return null;
            }
        }

        /*
            Function removes a product from an order. This will be used when an order is closed and it contains a product that is out of stock. That product will be removed from that order before it is closed.
            Parameters: orderId, productId
            Returns a boolean value depending on if the removal was successful.
        */
        public bool RemoveProductFromOrder(int orderId, int productId)
        {
            //update order record in Database
            string SQLUpdate = $@"DELETE FROM OrderProduct
            WHERE OrderId = {orderId}
            AND ProductId = {productId};";

            // initialize a variable to hold whether order was successfully updated
            bool success;
            // try to update database
            try
            {
                _db.Update(SQLUpdate);
                success = true;
            }
            catch (Exception err)
            {
                Console.WriteLine("Remove Product Error", err.Message);
                success = false;
            }
            // return whether db update was successful
            return success;
        }

        /* 
          Author: Krys Mathis
            Summary: Queries the database and returns the available quantity for a product
                    this is the result of the product quantity minus the total number of 
                    order rows for the product on closed orders (PaymentTypeId is not null)
            Parameter: Product object
            Returns: amount of available product (initial minus sold)
         */
        public int getAvailableQuantity(Product product)
        {
            int initialQuantity = product.Quantity;
            int soldQuantity = 0;

            _db.Query($@"SELECT Count(*) as Sold 
					    FROM
                        OrderProduct op, 
						`Order` o
						WHERE o.CompletedDate is not null
						AND op.OrderId = o.Id
                        AND op.ProductId = {product.Id};",

                     (SqliteDataReader reader) =>
                            {
                                while (reader.Read())
                                {
                                    // if the value is null, do not reassign it
                                    if(!reader.IsDBNull(0))
                                    {
                                        soldQuantity = Convert.ToInt32(reader["Sold"]);
                                    }
                                }
                    });
                    
            return initialQuantity - soldQuantity;
        }

        /*
            Author: Krys Mathis
            Summary: Checks if available quantity is > 0
            Parameter: Product object
            Returns: true of false
         */
        public bool hasAvailableQuantity(Product product){
            return getAvailableQuantity(product) > 0;
        }
    }
}