using System;
using System.Linq;
using bangazon_cli.Models;
using bangazon_cli.Managers;
using System.Collections.Generic;

namespace bangazon_cli.Menus
{
    /*
        Author: Krys Mathis
        Summary: Guides user through the active customers current items
                 and stops them from deleting items if on an active order.
    */
    public class DeleteActiveCustomerProductsMenu
    {
        private Customer _activeCustomer;
        private ProductManager _productManager;

        public DeleteActiveCustomerProductsMenu(Customer activeCustomer, ProductManager productManager)
        {
            _activeCustomer = activeCustomer;
            _productManager = productManager;
        }

        /*
            The show method loops until the user takes the exit path
            It guides the user through the process to delete products
         */
        public void Show() {
            
            int exitChoice = 0;
            bool exitMenu = false;
            do {
                Console.Clear();
                Console.WriteLine("Choose product to delete:");

                // get the active customer's products
                // TODO: Use productManager's getCustomersProducts method 
                List<Product> products = _productManager.GetProducts()
                                .Where(p => p.CustomerId == _activeCustomer.Id)
                                .OrderBy(x => x.Name).ToList();
                
                exitChoice = products.Count()+1;
                
                // Get the selected product from the user
                int choice = GetChoice(products);

                // logic to validate the choice
                if (choice == 0 || choice == exitChoice) {
                    return;
                } 
                
                // get the product from the products list
                Product product = products.ElementAt(choice -1);
                
                // Check if it is on an unpaid order
                if (_productManager.IsProductOnOrder(product.Id)) {
                    Console.WriteLine("This product is on an order - cannot delete");
                
                } else {
                    // delete the product
                    _productManager.DeleteProduct(product.Id);
                    Console.WriteLine("Product deleted!");
                }
                
                Console.WriteLine("Press any key to continue");
                Console.ReadLine();

                

            } while (exitMenu == false);
        }

        // Displays the list of products and returns the id of the chosen
        // product or the exit choice
        public int GetChoice(List<Product> products) {
            
            int choice = -1;

            // make sure there are products to display
            if (products.Count() == 0 ) {
                Console.WriteLine("No products to delete - press any key to continue");
                Console.ReadLine();
                return 0;
            }

            do {
                Console.Clear();
                int index = 1;
                products.ForEach(c => {
                    Console.WriteLine($"{index}. {c.Name}");
                    index ++;
                });

                Console.WriteLine($"{index}. Exit");
                Console.Write("> ");
                string userSelection = Console.ReadLine();
                
                // only reassign the product if they didn't select the exit path
                if (!int.TryParse(userSelection, out choice)) {  
                    choice = -1;
                }
            // while check that a choice was made. This could include the exit path
            } while (choice == -1 || choice > products.Count()+1);

            return choice;
            
        }
    }
}