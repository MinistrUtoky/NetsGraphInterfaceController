   M S B A M L   `   `   `  ����X  TInterfaceForGraphCalculations, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullM+clr-namespace:InterfaceForGraphCalculationsInterfaceForGraphCalculations  5   	   .    (InterfaceForGraphCalculations.DataWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   =local+clr-namespace:InterfaceForGraphCalculations     5   	     � 
ResizeMode$  CanMinimize=�5       � Title$ 
MainWindow��5   	   $	��450��6   $	��800��6)   .��5	      �  � ColumnDefinitions 5
   
   �� 5      $��1* �6    � RowDefinitions 5   
   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $	��18* �6   .+�5   
   o� -   5         o�Name$ menuGrid��5      $��0��6"   $��20��6/   $	��Top=�6;   5      � 5   
   -   5         �Name$ headGrid��5      $��1��6"    5      �� 5      $��1* �6#   �� 5      $��1* �6#   �� 5      $��1* �6#   �� 5      $��1* �6#   �� 5      $��1* �6#   �� 5      $��1* �6#   �� 5      $��1* �6#   �� 5      $��1* �6#   �� 5       $��1* �6#   �� 5!      $��1* �6#   �� 5"      $��1* �6#   �� 5#      $��1* �6#   .+�5%      �� $
��Bold"�6   $��14(�6*   $��Center=�68   $��2��6S   $��Center=�6g   $��TABLES��6�   5&      � 5(   
   -   5        $ databaseInfoGrid��5(      $��2��6)    5)      �� 5*      -   5         ��Name$ tablesTreeColumn��5*   #   $��1* �6=   �� 5+      -   5        $ tableColumn��5+   #   $��5* �68   .+�5.      R� -   5         R�Name$ tableNamesTree��5.      $��1��6/   5/      &   System.Windows.Controls.DataGrid  50      -   5          Name$ 	tableGrid��50      $��1��6*   	   
IsReadOnly$
	 True��6:   � 51      -   5        
 �Name$
 	Add_Popup��51      $��Center=�6'   $��Center=�6:   $��Center=�6W   $	��200��6r   $	��200��6~   $��False��6�    � 	StaysOpen$ False��6�   52      � 53      -	   5        $
 Update_Popup��53      $��Center=�6*   $��Center=�6=   $��Center=�6Z   $	��200��6u   $	��200��6�   $��False��6�   $ False��6�   54      � 55      -
   5        $
 Remove_Popup��55      $��Center=�6*   $��Center=�6=   $��Center=�6Z   $	��200��6u   $	��200��6�   $��False��6�   $ False��6�   56      � 57      -   5        $
 New_Table_Popup��57      $��Center=�6-   $��Center=�6@   $��Center=�6]   $	��200��6x   $	��200��6�   $��False��6�   $ False��6�   58      � 59      -   5        $
 New_Table_subPopup��59      $��Center=�60   $��Center=�6C   $��Center=�6`   $	��200��6{   $	��200��6�   $��False��6�   $ False��6�   5:      5;      5<      5=      