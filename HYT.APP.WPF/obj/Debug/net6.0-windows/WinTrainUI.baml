   M S B A M L   `   `   `  ����C  ?SWBT.APP, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:HYT.APP.WPFHYT.APP.WPF  5   	   ?$clr-namespace:HYT.APP.WPF.UIControlsHYT.APP.WPF.UIControls  5   	       HYT.APP.WPF.WinTrainUI   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   +localclr-namespace:HYT.APP.WPF     5   	   5ctrl$clr-namespace:HYT.APP.WPF.UIControls     5   	   -   5          � WindowState$  	Maximized=�5       � WindowStyle$
 None=�62    � 
ResizeMode$ NoResize=�6E    � AllowsTransparency$
 True��6[    � Title$ 
WinTrainUI��5	   	   $
��1080��6   $
��1920��6*   
������  � 	Resources 5
         movePath��%=   &	        B� 5   
   $��M -300,-100 L400,100C�6(   6I   5      .��5      � .+�5   
   (�  K� ZIndex$	 100��6   .��5      � -   5        $��gridMain��5      $
��1920��6#   $
��1080��60   ���	#01000000 � RowDefinitions 5      �� 5      $��80 �6$   62   �� 5      $��* �6$   61   .+�5      �� -   5        $��lblServerState��5   ;   $��10(�6   $��5��6*   
�������$��Bottom=�6d   $��3q�6   6�   � 5      -   5        $��gridDeviceDisconnect��5      $��3��63   ���	#70000000$y�	Collapsed=�6[   .+�5      �� $
��1200��6   $	��700��6*   
������� �� CornerRadius$ 15��6J   ._�5      �  5      �� 5   "   $��* �60   6=   �� 5   "   $	��100 �60   6?   .+�5!      �� 	 �� TextAlignment$	 Center=�6(   $��50(�6?   $��Center=�6M   #!设备断开连接，训练暂停6u   �� 5"      -   5        $��1��5"   @   $	��100��6M   $��40��6Y   $��16(�6e   .��6s   退出6w   5#      5%      5&      �� 5+      -   5        $��gridLoad��5+      $��0��6)   $��3��66   $y�Visible=�6G   ��5,      �� 5-      
 �� ImageSource$*
 $/Content/Resource/Image/200001BG.png��6%   .+�51      �� -   5        $��imgProgress��51   J   $��0��6   
 �� Top$	 850��6'    �� Left$	 110��68   $��40��6]   $��UniformToFill=�6j   �� 54      -   5        $��loadText��54   U   $
��1920��6    $	 910��6-   $	 Center=�6>   
�������$��24(�6x   $��Center=�6�   $��Bottom=�6�   训练加载中...0%6�   58      5;      5<      5?      5@      