// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

#region using

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using FirstFloor.ModernUI.Windows.Controls;

using MANAGER.Classes;
using MANAGER.ComboBox;
using MANAGER.Properties;

using Category = MANAGER.Table.Category;

#endregion

namespace MANAGER.Pages
{
    public partial class EstimatePage
    {
        private static readonly List<Merchandise> ListMerchandise = new List<Merchandise>();
        private readonly Estimate _estimate = new Estimate(ListMerchandise);
        private double _itemSelectedPrice;
        private int _itemSelectedQuantity;
        private double _totalCost;

        /// <summary>
        /// Method is called when the page is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EstimateCreator_Loaded(object sender, RoutedEventArgs e)
        {
            //the function UpdateText is called to apply new traduction
            UpdateText();
            //Apply new border brush to every grid
            var nbMerchandise = _estimate.GetList.Count;
            for(var i = 0; i < nbMerchandise; i++)
            {
                _estimate[i].Border.BorderBrush = BtnAdd.BorderBrush;
            }
            //Update textblock
            QuantityChanged();
        }

        /// <summary>
        /// Methode is called when the page is initialized (so it's only called once)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxCategory_Initialized(object sender, EventArgs e)
        {
            //Try to initialized, if can't, show a messagebox
            try
            {
                //Insert into the value into the combobox (every category found in the database)
                var command = Connection.Connection.GetAll(Category.TableName);
                var resultat = command.ExecuteReader();
                while(resultat.Read())
                {
                    ComboBoxCategory.Items.Add(new ComboboxItemCategory
                    {
                        Text = resultat[Category.Title].ToString(),
                        Value = new Classes.Category(Convert.ToInt32(resultat[Category.ID]), resultat[Category.Title].ToString())
                    });
                }
                resultat.Close();
            }
            catch
            {
                ModernDialog.ShowMessage(Transharp.GetTranslation("Box_DBFail"), Transharp.GetTranslation("Box_Error"), MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// When the user select another item in the ComboBoxCatergory, this method is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Try to initialized, if can't, show a messagebox
            try
            {
                //Clearthe ComboBoxProduct
                ComboBoxProduct.Items.Clear();
                //Select every product that are 'onSale' and those who are in the selected category
                var query =
                    $"{Table.Merchandise.TableName} WHERE {Table.Merchandise.OnSale} = 1 AND {Table.Merchandise.Quantity} > 0 AND ID_{Category.TableName}={((ComboboxItemCategory) ComboBoxCategory.SelectedItem).Value.Id}";
                var command = Connection.Connection.GetAll(query);
                var result = command.ExecuteReader();
                while(result.Read())
                {
                    //Insert into the value into the combobox (every product returned with the query)
                    ComboBoxProduct.Items.Add(new ComboboxItemMerchandise
                    {
                        Text = result[Table.Merchandise.Name].ToString(),
                        Value =
                            new Merchandise(Convert.ToInt32(result[Table.Merchandise.ID]), result[Table.Merchandise.Name].ToString(),
                                Convert.ToInt32(result[Table.Merchandise.Price]), Convert.ToInt32(result[Table.Merchandise.Quantity]),
                                ((ComboboxItemCategory) ComboBoxCategory.SelectedItem).Value.Id)
                    });
                }
                //Close the query
                result.Close();
                //Select the first product
                ComboBoxProduct.SelectedIndex = 0;
            }
            catch
            {
                ModernDialog.ShowMessage(Transharp.GetTranslation("Box_DBFail"), Transharp.GetTranslation("Box_Error"), MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// When the user select another item in the ComboBoxProduct, this method is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Change by default the text of the button
            BtnAdd.Content = Transharp.GetTranslation("BTN_Add");
            try
            {
                //Switch the value of _itemSelectedQuantity (quantity of item the user wants)
                switch(_itemSelectedQuantity)
                {
                    //If it's 0, then call ErrorCost()
                    case 0:
                        ErrorCost();
                        break;
                    //If the value is correct :
                    default:
                        //Multiply the price of the selected item by the quantity
                        _itemSelectedPrice = ((ComboboxItemMerchandise) ComboBoxProduct.SelectedItem).Value.Price * Convert.ToInt32(TextBoxEstimateQte.Text);
                        //Show it
                        AllPrice.Text = $"{Transharp.GetTranslation("All_Price")} {_itemSelectedPrice}€";

                        //If the selected product is already in the estimate, show "modify" instead of "add" in the button
                        var nbMerchandise = _estimate.GetList.Count;
                        for(var i = 0; i < nbMerchandise; i++)
                        {
                            if(_estimate[i].Id == ((ComboboxItemMerchandise) ComboBoxProduct.SelectedItem).Value.Id)
                            {
                                BtnAdd.Content = Transharp.GetTranslation("BTN_Modify");
                            }
                        }
                        break;
                }
            }
            catch(Exception caught)
            {
                Console.WriteLine(caught.Message);
                Console.Read();
            }
        }

        /// <summary>
        /// Methode is called when the page is initialized (so it's only called once)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxClient_OnInitialized(object sender, EventArgs e)
        {
            //Try to initialized, if can't, show a messagebox
            try
            {
                //Get all customer in the database
                var command = Connection.Connection.GetAll(Table.Customer.TableName);
                var resultat = command.ExecuteReader();
                while(resultat.Read())
                {
                    //Insert into the value into the combobox (every customer found in the database)
                    ComboBoxClient.Items.Add(new ComboboxItemCustomer
                    {
                        Text = resultat[Table.Customer.Name].ToString(),
                        Value =
                            new Customer(Convert.ToInt32(resultat[Table.Customer.ID]), resultat[Table.Customer.Name].ToString(),
                                resultat[Table.Customer.Phone].ToString(), resultat[Table.Customer.Email].ToString())
                    });
                }
                resultat.Close();
                ComboBoxClient.SelectedIndex = 0;
            }
            catch
            {
                ModernDialog.ShowMessage(Transharp.GetTranslation("Box_DBFail"), Transharp.GetTranslation("Box_Error"), MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// When the user select another item in the ComboBoxClient, this method is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxClient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Change the customer by the one selected
            _estimate.Customer = ((ComboboxItemCustomer) ComboBoxClient.SelectedItem).Value;
        }

        /// <summary>
        /// When the user change the text in the TextBoxEstimateQte
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxEstimateQte_TextChanged(object sender, TextChangedEventArgs e)
        {
            //If there isn't a try/catch, the app crash, don't know why
            try
            {
                //Call the QuantityChanged method
                QuantityChanged();
            }
            catch(Exception caught)
            {
                Console.WriteLine(caught.Message);
                Console.Read();
            }
        }

        /// <summary>
        /// When the user click on the BTNAddFeed button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTNAddFeed_click(object sender, RoutedEventArgs e)
        {

            var text = $"{ComboBoxCategory.Text} - {ComboBoxProduct.Text}";
            var nbMerchandise = _estimate.GetList.Count;
            for(var i = 0; i < nbMerchandise; i++)
            {
                //If the product is already in the estimate call UpdateEstimate with its key on the estimate and with 'null' as id
                if(_estimate[i].Name != text)
                {
                    continue;
                }
                UpdateEstimate(i, null);
                return;
            }
            //call the method AddMerchandise
            AddMerchandise(((ComboboxItemMerchandise)ComboBoxProduct.SelectedItem).Value.Id, text, _itemSelectedQuantity, _itemSelectedPrice,
                ((ComboboxItemMerchandise) ComboBoxProduct.SelectedItem).Value.CategoryId);
            AjouterEstimate.IsEnabled = true;
            //Change the button to 'Modify'
            BtnAdd.Content = Transharp.GetTranslation("BTN_Modify");
        }

        /// <summary>
        /// When the user click on BTNAddEstimate, this method is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTNAddEstimate_click(object sender, RoutedEventArgs e)
        {
            //Initialising this var
            var numberEstimate = 0;
            try
            {
                //As there isn't auto inc in oracle so here's an "auto inc" like
                var querySelect = string.Format("SELECT max(ID_{0}), max({1}) FROM {0}", Table.Estimate.TableName, Table.Estimate.NumberDevis);
                var oracleCommand = Connection.Connection.Command(querySelect);
                var result = oracleCommand.ExecuteReader();
                var sizeList = ListMerchandise.Count;
                while(result.Read())
                {
                    var idEstimate = result[0].ToString() == string.Empty ? 1 : Convert.ToInt32(result[0]) + 1;
                    numberEstimate = result[1].ToString() == string.Empty ? 1 : Convert.ToInt32(result[1]) + 1;
                    //For each product in the estimate, add it to the database
                    for(var i = 0; i < sizeList; i++)
                    {
                        Connection.Connection.Insert(Table.Estimate.TableName, _estimate.Customer.Id, _estimate[i].Id, ((idEstimate) + i), _estimate[i].Quantity,
                            DateTime.Now.ToString("dd/MM/yy"), _estimate[i].Price, (numberEstimate));
                    }
                }
                result.Close();
                
                //Show success message
                ModernDialog.ShowMessage(Transharp.GetTranslation("Box_SuccessAdd", numberEstimate, _totalCost), Transharp.GetTranslation("Box_CE_Success"),
                    MessageBoxButton.OK);

                //Reset the page
                BtnAdd.Content = Transharp.GetTranslation("BTN_Add");
                PanelEstimate.Children.Clear();
                ListMerchandise.Clear();
                _totalCost = 0;
                LabelTotalPrix.Text = string.Empty;
                AjouterEstimate.IsEnabled = false;
            }
            catch
            {
                ModernDialog.ShowMessage(Transharp.GetTranslation("Box_DBFail"), Transharp.GetTranslation("Box_Error"), MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// When the user change the page's size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Adjust the border size
            BorderEstimate.Width = EstimateCreator.ActualWidth - 340;
            BorderEstimate.Height = EstimateCreator.ActualHeight - 70;

            var nbMerchandise = _estimate.GetList.Count;
            for(var i = 0; i < nbMerchandise; i++)
            {
                _estimate[i].Border.Width = BorderEstimate.Width - 6;
            }
        }

        /// <summary>
        /// Dynamicly created button, this method is called when the user click on it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTN_Delete_Click(object sender, EventArgs e)
        {
            //Update the estimate by suppressing one merchandise
            UpdateEstimate(null, Convert.ToInt32(((Button) sender).Tag.ToString()));
        }

        /// <summary>
        /// Method which show a merchandise on the grid
        /// </summary>
        /// <param name="id">merchandise's id</param>
        /// <param name="name">merchandise's name</param>
        /// <param name="quantity">quantity of the merchandise the user wants</param>
        /// <param name="price">merchandise's price</param>
        /// <param name="category">merchandise's category</param>
        private void AddMerchandise(int id, string name, int quantity, double price, int category)
        {
            //Create a new panel
            var panelMerchandise = new StackPanel();
            var newMerchandise = new Merchandise(id, name, quantity, price, category);
            //Init default thick
            var thick = new Thickness(5, 2, 0, 0);

            //Get default color
            var convertFromString = ColorConverter.ConvertFromString(Settings.Default.AccentColor);
            if(convertFromString != null) {

                //Create a graphical border
                var border = new Border
                {
                    BorderBrush = new SolidColorBrush((Color) convertFromString),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(2, 2, 1, 0),
                    BorderThickness = new Thickness(1),
                    Width = BorderEstimate.Width - 6,
                    Child = panelMerchandise,
                    Height = 70
                };

                // Merchandise's name textblock
                panelMerchandise.Children.Add(new TextBlock {HorizontalAlignment = HorizontalAlignment.Left, Margin = thick, Text = name, Height = 16});

                // Merchandise's price textblock
                panelMerchandise.Children.Add(new TextBlock
                {
                    Text = $"{price}€",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = thick,
                    Height = 16
                });

                // Merchandise's quantity textblock
                panelMerchandise.Children.Add(new TextBlock
                {
                    Text =
                        $"{Transharp.GetTranslation("EC_Quantity")} : {quantity.ToString(CultureInfo.InvariantCulture)}",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = thick,
                    Height = 16
                });

                // Delete button
                var btnDelete = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Content = Transharp.GetTranslation("EC_DeleteMerchandise"),
                    Margin = new Thickness(9, -30, 67, 50),
                    BorderBrush = Brushes.Red,
                    Tag = newMerchandise
                };

                //Add to the panel
                panelMerchandise.Children.Add(btnDelete);
                btnDelete.Click += BTN_Delete_Click;

                newMerchandise.Border = border;
                PanelEstimate.Children.Add(border);
            }
            _estimate.GetList.Add(newMerchandise);
            _totalCost += price;
            LabelTotalPrix.Text = Transharp.GetTranslation("All_Total", _totalCost);
        }

        /// <summary>
        /// Method is called when the quantity is wrong (negative, decimal, string,  ....)
        /// </summary>
        private void ErrorCost()
        {
            //Change textblock by an error message
            AllPrice.Text = $"{Transharp.GetTranslation("All_Price")} {Transharp.GetTranslation("Box_Error")}";
            //Disable the add button
            BtnAdd.IsEnabled = false;
            //Put textblock and textbox in red
            AllPrice.Foreground = TextBoxEstimateQte.CaretBrush = TextBoxEstimateQte.SelectionBrush = TextBoxEstimateQte.BorderBrush = Brushes.Red;
        }

        /// <summary>
        /// Method is called when the quantity changed and when the page is loaded
        /// </summary>
        private void QuantityChanged()
        {
            //init quantity
            _itemSelectedQuantity = 0;

            //If the text in the TextBoxEstimateQte is a valid int, if not, call ErrorCost()
            if(IsInt(TextBoxEstimateQte.Text))
            {
                //Convert it to a int
                var newQuantity = Convert.ToInt32(TextBoxEstimateQte.Text);

                //If it's less or equals to 0 call the method ErrorCost()
                if(newQuantity <= 0)
                {
                    ErrorCost();
                }
                else
                {
                    //If there isn't product selected call the method ErrorCost()
                    if(ComboBoxProduct.Items.Count == 0)
                    {
                        ErrorCost();
                    }
                    else
                    { 
                        _itemSelectedQuantity = newQuantity;
                        _itemSelectedPrice = (((ComboboxItemMerchandise) ComboBoxProduct.SelectedItem).Value.Price * newQuantity);
                        AllPrice.Foreground = LabelTotalPrix.Foreground;
                        AllPrice.Text = $"{Transharp.GetTranslation("All_Price")} {_itemSelectedPrice}€";
                        var convertFromString = ColorConverter.ConvertFromString(Settings.Default.AccentColor);
                        if(convertFromString != null)
                        {
                            TextBoxEstimateQte.BorderBrush =
                                TextBoxEstimateQte.CaretBrush = TextBoxEstimateQte.SelectionBrush = new SolidColorBrush((Color) convertFromString);
                        }
                        BtnAdd.IsEnabled = true;
                    }
                }
            }
            else
            {
                ErrorCost();
            }
        }

        /// <summary>
        /// Methode that verify if a string is a int or not 
        ///     (check from textbox, ...)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool IsInt(string str)
        {
            int value;
            //Return true if int
            return (str.Trim() != string.Empty) && int.TryParse(str, out value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idUpdate">Null if delete</param>
        /// <param name="idDelete">Null if update</param>
        private void UpdateEstimate(int? idUpdate, int? idDelete)
        {
            //Initialising vars
            var listMerchandiseN2 = new List<Merchandise>();
            BtnAdd.Content = Transharp.GetTranslation("BTN_Add");
            _totalCost = 0;
            LabelTotalPrix.Text = string.Empty;

            var nbMerchandise = _estimate.GetList.Count;
            for(var i = 0; i < nbMerchandise; i++)
            {
                if (idUpdate.HasValue)
                {
                    if (i == idUpdate.Value)
                    {
                        var text = $"{ComboBoxCategory.Text} - {ComboBoxProduct.Text}";
                        var merchandiseCost = _itemSelectedPrice;
                        listMerchandiseN2.Add(new Merchandise(((ComboboxItemMerchandise) ComboBoxProduct.SelectedItem).Value.Id, text, _itemSelectedQuantity,
                            merchandiseCost, ((ComboboxItemMerchandise) ComboBoxProduct.SelectedItem).Value.CategoryId));
                    }
                    else
                    {
                        listMerchandiseN2.Add(ListMerchandise[i]);
                    }
                }
                else
                {
                    if (ListMerchandise[i].ToString() != idDelete.ToString())
                    {
                        listMerchandiseN2.Add(ListMerchandise[i]);
                    }
                }
            }
            if (idDelete.HasValue)
            {
                nbMerchandise -= 1;
            }

            for(var i = 0; i < nbMerchandise; i++)
            {
                if(listMerchandiseN2[i].Id == ((ComboboxItemMerchandise) ComboBoxProduct.SelectedItem).Value.Id)
                {
                    BtnAdd.Content = Transharp.GetTranslation("BTN_Modify");
                }
            }

            PanelEstimate.Children.Clear();
            _estimate.GetList.Clear();

            for(var i = 0; i < nbMerchandise; i++)
            {
                AddMerchandise(listMerchandiseN2[i].Id, listMerchandiseN2[i].Name, listMerchandiseN2[i].Quantity, listMerchandiseN2[i].Price,
                    listMerchandiseN2[i].CategoryId);
            }
            listMerchandiseN2.Clear();
            if(_estimate.GetList.Count == 0)
            {
                AjouterEstimate.IsEnabled = false;
            }
        }

        /// <summary>
        /// Method that update the text language
        /// </summary>
        private void UpdateText()
        {
            EcTitle.Text = Transharp.GetTranslation("EC_Title");
            EcChooseCategory.Text = Transharp.GetTranslation("EC_ChooseCategory");
            EcAddMerchandise.Text = Transharp.GetTranslation("EC_AddMerchandise");
            AllQte.Text = Transharp.GetTranslation("All_QTE");
            AllPrice.Text = Transharp.GetTranslation("All_Price");
            EcCustomer.Text = Transharp.GetTranslation("EC_Customer");
            BtnAdd.Content = Transharp.GetTranslation("BTN_Add");
            AjouterEstimate.Content = Transharp.GetTranslation("BTN_Create");
        }
    }
}