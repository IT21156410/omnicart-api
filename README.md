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

**Fonseka M.M.N.H. - IT21156410**:

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
    
