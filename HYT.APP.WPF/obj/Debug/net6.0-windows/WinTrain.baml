   M S B A M L   `   `   `  ����C  ?SWBT.APP, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:HYT.APP.WPFHYT.APP.WPF  5   	       HYT.APP.WPF.WinTrain   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   +localclr-namespace:HYT.APP.WPF     5   	     � WindowState$  	Maximized=�5       � WindowStyle$
 None=�62    � 
ResizeMode$ NoResize=�6E    � AllowsTransparency$
 True��6[    � Title$ 训练��5   	   $
��1080��6   $
��1920��6"    � ShowInTaskbar$ False��6/    J� IsHitTestVisible$
 True��6E    � 	Resources 5	         FocusVisual �� Property	 �� Value
 J� SnapsToDevicePixels �� StrokeDashArray  Button.Static.Background �� Color  Button.Static.Border  Button.MouseOver.Background  Button.MouseOver.Border  Button.Pressed.Background  Button.Pressed.Border  Button.Disabled.Background  Button.Disabled.Border 	 Button.Disabled.Foreground 
 ButtonStyle1 �� 
TargetType �� 
TargetType ��Name ��Name �� Triggers P� Property P� Value �� 
TargetName��%>  &	        &	 �     &	 �     &	 �     &	     &	 ?    &	 b    &	 �    &	 �    &		 �    &	
 �    7   7  7  7  7  7 	 �� 5
   
   .�5      ��  � ��	 5      �� 5      .R�5      �� $��2q�6$   $

 true��6/   #��� ��6J   $��1��6�   $	 1 2R�6�   5      5      5      �� 5   
   $ 	#FFDDDDDD��6;   �� 5   
   $ 	#FF707070��67   �� 5   
   $ 	#FFBEE6FD��6>   �� 5   
   $ 	#FF3C7FB1��6:   �� 5   
   $ 	#FFC4E5F6��6<   �� 5   
   $ 	#FF2C628B��68   �� 5   
   $ 	#FFF4F4F4��6=   �� 5   
   $ 	#FFADB2B5��69   �� 5   
   $ 	#FF838383��6=   �� 5   
   " ��6%   .�5      ��  �@�FocusVisualStyle8	   61   �� 5       � ��8	  6+   �� 5       � ��8	  6,   �� 5        � ��#	 � ��6+   �� 5!       � ��$	 1q�60   �� 5"       � ��$	 Center=�6;   �� 5#       � ��$	 Center=�69   �� 5$       � ��$	 1q�6(   �� 5%       � ��	 5&      �� 5'      " ��6&   .R�5(      �� $ border��6!   #��z��61   #��z��6]   #��z��6�   $

 true��6�   ._�5)      �� $ contentPresenter��6/   $|�False��6I   #��z��6[   #��z��6�   $
��True��6�   #
 z
 6�   #��z��6  5*       5+      P� 5,       �@��IsDefaulted$
 true��6=   . �5-   "   �� $ border��6@   #	 � ��6T    � ��5.      P� 50       � {�$ false��6;   . �51   "   �� $ border��6?   8	   � ��6S   �� 52   "   $ border��6@   8	   � ��6T   �� 53   "   $ contentPresenter��6K   8	   � ��6i   54      56      58      59      5:      .��5<      (� .��5=   
   �� -   5        $��gridRoot��5=   ,   $
��1920��6   $
��1080��6   5@      5A      5B      