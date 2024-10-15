# C# ASP.NET Web API - Omnicart

### **EAD SE4040 - ASSIGNMENT**

This repository contains the **Omnicart** web application, which handles user management, product management, inventory management, and vendor management. The system is built using **ASP.NET Core** and includes a RESTful API to manage products, vendors, users, and notifications. Additionally, it covers mobile application features through API endpoints that allow customers to browse and purchase products.

## Project Scenario

The **Omnicart** system aims to facilitate the management of an e-commerce platform. It includes functionality for **administrators**, **vendors**, and **customer service representatives (CSR)**, enabling them to manage users, products, orders, and inventory efficiently. Customers can browse products, place orders, and manage their accounts through the web and mobile applications. The system includes inventory tracking with notifications for low stock and manages the entire product lifecycle, from creation to sale.

## Assignment Details

### 1. **User Management**
   - **Create and manage users** with distinct roles: Administrator, Vendor, and CSR.
   - Only **Administrators** can access functions related to managing vendors and inventory.

### 2. **Product Management**
   - **Vendors** can create, update, and delete products using a unique Product ID.
   - Product deletion is restricted if the product is part of any pending orders.
   - Vendors can **activate and deactivate** product listings.

### 3. **Inventory Management**
   - Track inventory levels for each product and **generate alerts** for low stock levels.
   - Notifications are sent to vendors when their product stock is low.
   - Vendors can automatically manage stock levels based on the number of available items.
   - Stock cannot be removed for products that are part of pending orders.

### 4. **Vendor Management**
   - **Vendor creation** is managed by the Administrator.
   - Vendors are responsible for managing product listings and inventory levels.

### 5. **Mobile Application (API)**
   - **Customer Account Management**:
     - Customers can modify and deactivate their accounts.
     - Accounts must be activated by either the Administrator or CSR before they can log in to the mobile application.
     - CSR can deactivate and reactivate accounts.
   - **Product Browsing and Purchasing**:
     - Customers can browse products by category, search for items, and view detailed product information.
     - Filtering and sorting based on **price**, **vendor**, **ratings**, and **product category** are provided.

### 6. **Notifications**
   - Notifications are sent to **Vendors** when their product stock is low or when important product updates occur.
   - Notifications for account-related activities (such as **account creation** or **activation**) are sent to **CSRs** for approval.

---

## Project Features

### a) **Web Application:**

#### 1. **User Management**:
   - **Administrator** role can create and manage user accounts for vendors and CSR.
   - **CSR** and **Vendor** roles have specific access and permissions for their respective management functionalities.
   
#### 2. **Product Management**:
   - Vendors can create, update, and delete products, ensuring that the product inventory is accurate.
   - Before a product can be deleted, the system checks whether any **pending orders** are associated with that product.

#### 3. **Inventory Management**:
   - Track the number of available products, **generate alerts for low stock**, and notify vendors when stock is below a certain threshold.
   - Stock cannot be reduced or removed for products that are part of pending orders.
   
#### 4. **Vendor Management**:
   - Vendors are created by Administrators, and vendors are responsible for managing their product listings and inventory.

### b) **Mobile Application:**

#### 1. **Account Management**:
   - Customers can modify their profiles and deactivate accounts.
   - Customer accounts must be approved by either the Administrator or CSR before they can log in to the mobile application.
   - **CSR** has the ability to reactivate deactivated accounts.

#### 2. **Product Browsing and Purchasing**:
   - Customers can browse products by category, filter by **price**, **vendor**, and **ratings**, and search for specific products.

---

## Contribution 

👨‍🎓**Fonseka M.M.N.H. - IT21156410**:

#### a) **User Management**:
- Created web application users with distinct roles: Administrator, Vendor, and Customer Service Representative (CSR). ✔️
- Restricted access to administrative functions (Vendor Management and Inventory Management) to Administrators. ✔️

#### b) **Product Management**:
- Allowed vendors to create, update, and delete products using a unique Product ID, with validations to ensure that orders are not pending for deletion. ✔️
- Enabled vendors to activate and deactivate product listings and categories. ✔️

#### d) **Inventory Management**:
- Implemented the management of inventory levels, allowing vendors to:
   - View how much stock is available for each product. ✔️
   - Track inventory levels and generate alerts for low stock, notifying vendors through the notification system. ✔️
   - Prevent stock removal for products that are part of pending orders. ✔️
- Automatically manage product stock based on the number of available items. ✔️

#### e) **Vendor Management**:
- Created the vendor management functionality, including the creation of vendor profiles, restricted to Administrators. ✔️

👨‍🎓**Prashantha K.G.M. - IT21169908**:

#### a) **User Management**:
- Allow customers to create their own accounts (using their email as the primary key) & Authenticate Users. ✔️
- Allow users to Two factor authentication & forgot password and password reset. ✔️

#### c) **Order Management**:
- Creating new orders (with order status tracking from processing to delivery). ✔️
- Updating order details (before the order is dispatched). ✔️
- Canceling orders (before the order is dispatched). ✔️
- Enable customers to track the status of their orders and view order history. ✔️

#### e) **Vendor Management**:
- Customer could set the rankings and the average ranking should be stored and visible to user. ✔️
- Customer comments for the vendor and the rankings cannot be modified. ✔️
- Customer could change the comment which they have created. ✔️
- Customer could rank the vendor based on the product purchase experience. ✔️
- Customer could comment on the vendor based on the product purchase experience. ✔️

#### g) **Customer Order Management**:
- CSR, Administrator could be able to see the status of customer order and cancel it when customer request to cancel. It should define a note when cancelling the Customer Order. Once the order is cancelled, it should inform to customer as a notification. ✔️
- CSR, Administrator and Vendor could be able to see the status of customer order and mark it as delivered. Once it is marked as delivered, it should be informed to Customer. ✔️
- If the Order contains products of multiple Vendors, Particular vendor can mark their product is ready and set the Order status as partially delivered. Once all vendors set the order as delivered, Order will be automatically move into delivered state. Other than that, CSR and Administrator could mark the Order as Delivered directly ( Assuming that, they collect goods from Vendors and deliver from their end or they get stocks from warehouse). ✔️
- Allow CSR to create order cancellation requests by customer phone call through requests. ✔️

#### b) **Product Browsing and Purchasing**:
- Enable customers to add products to their cart, place orders, and make payments. ✔️
- CSR or Administrator or Vendor could mark it as delivered. ✔️

---
### Technologies Used
  - ASP.NET Core 8.0
  - MongoDB for data persistence.
  - Entity Framework Core for data access.
  - JWT Authentication for secure user authentication.
  - React for front-end (BackOffice ERP).

---

## Running the Project

1. **Clone the repository**:
   ```bash
   git clone https://github.com/IT21156410/omnicart-api.git
2. **Open the project using VS 2022 Or JetBrain Rider IDEA**
     
3. **Run IIS Express Server**
    
