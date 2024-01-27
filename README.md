1) Insert item:  
  It can create new items. Item insertion form has at least 10 fields. The fields include at least one image, one date, one Boolean. Insertion of invalid data is allowed, so implement proper validation rules.
2) Update item:  
  It can update existing items. Update form allows to modify all the fields of the item except id (primary key field). Insertion of invalid data is not allowed, so implement proper validation rules.
3) Delete item:  
  It can delete existing items. If item is already used in orders, bookings etc. all related data also deleted.
4) Filter items:  
  System allows to display data relevant for your case. Data is taken from at least 5 tables. Filter form  contains 5 input fields (e.g., if item is a flat to rent search could be done by number of rooms, floor, address etc.).

    Search results support sorting in ascending and descending order. It should be possible to sort by at least 5 columns;
   
    Search results support paging;
   
    Each row in the results provide a link to view details on a separate page;
   
At the database level stored procedure is used.

6) Data export:  
It can export search results from the point 4) to CSV, XML and JSON. Generation of CSV, XML and JSON is done using stored procedure or function.
7) Data import:  
A web form that allows importing of items from the file. Import is possible from CSV, XML and JSON formatted files. Provide sample CSV, XML and JSON files for testing import.
