using System;
using Xunit;
using bangazon_cli.Models;
using System.Collections.Generic;
using System.Linq;

/*
    Author: Greg Lawrence
    Purpose: To test methods of the Order Manager such as creating order, adding product to order
*/

namespace bangazon_cli.Managers.Tests
{
    

    public class OrderManagerShould
    {
        // private instances of things needed in unit tests
        private DatabaseInterface _db;
        private Customer _customer;
        private readonly OrderManager _orderManager;
        private readonly ProductManager _productManager;
        private readonly CustomerManager _customerManager;
        private readonly PaymentTypeManager _paymentTypeManager;
        private Order _testOrder;
        private Product _testProduct;
        private PaymentType _paymentType1;

        // constructor for unit test
        public OrderManagerShould()
        {
            // create database path
            string testPath = System.Environment.GetEnvironmentVariable("BANGAZON_CLI_APP_DB_TEST");
            _db = new DatabaseInterface(testPath);

            // initialize managers to create db tables and use later in tests
            _orderManager = new OrderManager(_db);
            _productManager = new ProductManager(_db);
            _customerManager = new CustomerManager(_db);
            _paymentTypeManager = new PaymentTypeManager(_db);
            
            // create a new customer instance
            _customer = new Customer();
            // properties added to new customer
            _customer.Id = 1;
            _customer.Name = "G Lawrence";
            _customer.StreetAddress = "123 Somewhere";
            _customer.City = "Nashville";
            _customer.State = "TN";
            _customer.PostalCode = "37206";
            _customer.PhoneNumber = "8018959001";
            
            // create a new order instance
            _testOrder = new Order();
            // create a new product instance
            _testProduct = new Product()
            {
                CustomerId = _customer.Id,
                Name = "Bicycle",
                Price = 55.25,
                Description = "Awesome bike",
                Quantity = 1
            };
        }

        [Fact]
        public void AddOrder()
        {
            // add customer to DB and get id
            int CustomerId = _customerManager.AddCustomer(_customer);
            // add the customerId to _testOrder
            _testOrder.CustomerId = CustomerId;
            
            // add _testOrder to db
            int orderId = _orderManager.AddOrder(_testOrder);
            
            // retrieve the order
            Order retrievedOrder = _orderManager.GetUnpaidOrder(CustomerId);   
            // check if the fields all match between the order sent to the db and the order retrieved from the db
            Assert.Equal(retrievedOrder.Id, orderId);
            Assert.Equal(retrievedOrder.CustomerId, CustomerId);
            Assert.Equal(retrievedOrder.CompletedDate, _testOrder.CompletedDate);
            Assert.Equal(retrievedOrder.PaymentTypeId, _testOrder.PaymentTypeId);
        }

        [Fact]
        public void AddProductToOrder()
        {
            // add customer to DB and get id
            int CustomerId = _customerManager.AddCustomer(_customer);
            // add the customerId to _testOrder
            _testOrder.CustomerId = CustomerId;
            // add the customerId to _testProduct
            _testProduct.CustomerId = CustomerId;

            // add a test order
            int orderId = _orderManager.AddOrder(_testOrder);
            // add a test product
            int productId = _productManager.AddProduct(_testProduct);

            // add product to order by creating a record in the OrderProduct join table
            _orderManager.AddProductToOrder(orderId, productId);

            // Retrieve the order that was just added to db
            Product returnedProduct = _orderManager.GetSingleProductFromOrder(orderId, productId);
            // assert that the product stored on the order is the same product that we sent in. 
            Assert.Equal(returnedProduct.Id, productId);
            Assert.Equal(55.25, returnedProduct.Price);
            Assert.Equal(returnedProduct.Quantity, 1);
            Assert.Equal(returnedProduct.Name, "Bicycle");
            Assert.Equal(returnedProduct.Description, "Awesome bike");
        }

        // [Fact]
        // public void CloseOrder()
        // {   
        //     // add customer to DB and get id
        //     int CustomerId = _customerManager.AddCustomer(_customer);
        //     // add the customerId to _testOrder
        //     _testOrder.CustomerId = CustomerId;
        //     // add the customerId to _testProduct
        //     _testProduct.CustomerId = CustomerId;

        //     // Variable initialization for Payment Type
        //     _paymentType1 = new PaymentType();
        //     // Adds properties to the _paymentType instance
        //     _paymentType1.Type = "Mastercard";
        //     _paymentType1.AccountNumber = 12345678910;

        //     // add a test order
        //     int orderId = _orderManager.AddOrder(_testOrder);
        //     // add a test product
        //     int productId = _productManager.AddProduct(_testProduct);

        //     // Adds the _paymentType to the _paymentList in the paymentTypeManager, passes in a customerId
        //     int paymentId = _paymentTypeManager.AddNewPaymentType(_paymentType1, CustomerId);

        //     // Close the order by adding the payment type and currentDate
        //     DateTime currentDate = DateTime.UtcNow;
        //     _orderManager.closeOrder(paymentId, CustomerId, currentDate);

        //     // retrieve the order and Assert that the updated values match
        //     Order retrievedOrder = _orderManager.getOrderById(orderId);

        //     // Get the payment type info used on the order
        //     // since PaymentTypeId is currently a nullable int type, use GetValueOrDefault() to convert it to an int. 
        //     int retrievedPaymentTypeId = retrievedOrder.PaymentTypeId.GetValueOrDefault();
        //     PaymentType retrievedPaymentType = _paymentTypeManager.GetSinglePaymentType(retrievedPaymentTypeId);

        //     // Assert that the payment type matches what we used to close the order
        //     Assert.Equal("Mastercard", retrievedPaymentType.Type);
        //     Assert.Equal(12345678910, retrievedPaymentType.AccountNumber);
        // }

        [Fact]
        public void Dispose()
        {
            _db.DeleteTables();
        }
    }
}