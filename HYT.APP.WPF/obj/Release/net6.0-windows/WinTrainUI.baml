   M S B A M L   `   `   `  ���� C  ?SWBT.APP, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:HYT.APP.WPFHYT.APP.WPF  ?$clr-namespace:HYT.APP.WPF.UIControlsHYT.APP.WPF.UIControls      HYT.APP.WPF.WinTrainUI   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     +localclr-namespace:HYT.APP.WPF     5ctrl$clr-namespace:HYT.APP.WPF.UIControls     -     � WindowState$  	Maximized=� � WindowStyle$
 None=� � 
ResizeMode$ NoResize=� � AllowsTransparency$
 True�� � Title$ 
WinTrainUI��$
��1080��$
��1920��
������  � 	Resources    movePath��%*   &	        B� $��M -300,-100 L400,100C�.��� .+�(�  K� ZIndex$	 100��.��� -   $��gridMain��$
��1920��$
��1080�����	#01000000 � RowDefinitions �� $��80 ��� $��* �.+��� -   $��lblServerState��$��10(�$��5��
�������$��Bottom=�$��3q�� -   $��gridDeviceDisconnect��$��3�����	#70000000$y�	Collapsed=�.+��� $
��1200��$	��700��
������� �� CornerRadius$ 15��._��  �� $��* ��� $	��100 �.+��� 	 �� TextAlignment$	 Center=�$��50(�$��Center=�#!设备断开连接，训练暂停�� -   $��1��$	��100��$��40��$��16(�.��退出�� -   $��gridLoad��$��0��$��3��$y�Visible=����� 
 �� ImageSource$*
 $/Content/Resource/Image/200001BG.png��.+��� -   $��imgProgress��$��0��
 �� Top$	 850�� �� Left$	 110��$��40��$��UniformToFill=��� -   $��loadText��$
��1920��$	 910��$	 Center=�
�������$��24(�$��Center=�$��Bottom=�训练加载中...0%