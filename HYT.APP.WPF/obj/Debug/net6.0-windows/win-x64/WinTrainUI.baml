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
������� �� CornerRadius$ 15��6J   ._�5      �  5      �� 5   "   $��* �60   6=   �� 5   "   $	��100 �60   6?   .+�5!      �� 	 �� TextAlignment$	 Center=�6(   $��50(�6?   $��Center=�6M   #!设备断开连接，训练暂停6u   �� 5"      -   5        $��1��5"   @   $	��100��6M   $��40��6Y   $��16(�6e   .��6s   退出6w   5#      5%      5&      � 5+      -   5        $��gridLoad��5+      $��0��6'   $��3��64   $y�Visible=�6E   .+�5.      � -   5        $��gridLoad_EC��5.      $y�	Collapsed=�6.   ��50      �� 51      
 �� ImageSource$B
 </SWBT.APP;component/Content/Resource/Image/加载_背景.png��6)   .+�55      �� -   5         ��Name$ loadDiQiu_EC��55       $	��800��66   $	��800��6B   $B��</SWBT.APP;component/Content/Resource/Image/加载_地球.png��6O    J� RenderTransformOrigin$ 0.5,0.5&�6�   z�56      �� 57   "   -	   5        loadDiQiuRT_EC�� �� Angle$ 1Q�57   J   59      �� 5<      -
   5        $ loadFeiJi_EC��5<       $	��108��66   $	��203��6B   $B��</SWBT.APP;component/Content/Resource/Image/加载_飞机.png��6O   $ 0.5,0.5&�6�   z�5=      T� 5>   "   -   5        loadFeiJiRT_EC�� T� X$	 400Q�5>   M    T� Y$	 100Q�6U   5@      5C      �� 5F      
������ $
��1600��66   $	��100��6C   $��160,887,160,100q�6P   $��Bottom=�6i   .+�5G      �� -   5        $��loadProgressAnim��5G        �� Left$	 -50��68   $	��100��6J   $	��100��6V   $	 0,0&�6c   z�5H      T� 5I   "   -   5        loadProgressAnimTT��$ 0Q�5I   Q   5K      5L      � 5O      -   5        $��progressLoad��5O   "   $
��1600��66   $��12��6C    � Maximum$ 1Q�6O    � Value$ 0Q�6[   $��Center=�6e   $��Bottom=�6�   $��0,0,0,81q�6�   5R      �� 5T      -   5        $��loadText��5T       
�������$��24(�6C   $��Center=�6Q   $��Bottom=�6n   $��0,0,0,35q�6�   训练加载中...0%6�   5X      5[      5\      5_      5`      