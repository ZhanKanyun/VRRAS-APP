   M S B A M L   `   `   `  ���� C  ?SWBT.APP, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:HYT.APP.WPF.UIControlsHYT.APP.WPF.UIControls  *    $HYT.APP.WPF.UIControls.VolumeControl   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     6local$clr-namespace:HYT.APP.WPF.UIControls     -     � 	Resources   !  SliderThumb.Static.Foreground �� Color $  SliderThumb.MouseOver.Background   SliderThumb.MouseOver.Border " SliderThumb.Pressed.Background  SliderThumb.Pressed.Border # SliderThumb.Disabled.Background  SliderThumb.Disabled.Border ! SliderThumb.Static.Background  SliderThumb.Static.Border 	 SliderThumbHorizontalTop �� 
TargetType � UseLayoutRounding F�Name �� Stretch J� SnapsToDevicePixels �� Triggers P� Property	 P� Value
 �� 
TargetName �� Value �� Property 
 SliderThumbHorizontalBottom  SliderThumb.Track.Border   SliderThumb.Track.Background  RepeatButtonTransparent �� 
TargetType   SliderThumbHorizontalDefault  SliderHorizontal ��Name � RowDefinitions m�Name m� Fill m� 	Placement ��Name ]�Name ]� DecreaseRepeatButton �� DecreaseLarge ]� IncreaseRepeatButton �� IncreaseLarge ]� Thumb n�Name � OverridesDefaultStyle �� CornerRadius J� Effect3  -System.Windows.Media.Effects.DropShadowEffect   ShadowDepth   
BlurRadius    Opacity!   Color  SliderThumbVerticalLeft  SliderThumbVerticalRight  SliderThumbVerticalDefault  SliderVertical" � ColumnDefinitions  SliderStyle1# �� Triggers��%  &	        &	      &	 *     &	 ?     &	 T     &	 i     &	 ~     &	 �     &	 �     &		 �     7  7  7  7  7  7  7  7  &	
 �    7  7  7  7  7  7  7  7  &	     &	 *    &	 ?    &	 �    7  7  7  7  7  7  7  7  &	     7  7  7  7  7 	 7 
 &	 �    7  7  7  7  7  7  7  7  &	 �    7  7  7  7  7  7  7  7  &	 �    7  7  7  7  7  7  7  7  &	 {    7  7  7  7  7  7  7  &	 s    7   7  7  �� $ 	#FFE5E5E5���� $ 	#FFDCECFC���� $ 	#FF7Eb4EA���� $ 	#FFDAECFC���� $ 	#FF569DE5���� $ 	#FFF0F0F0���� $ 	#FFD9D9D9���� $ 	#FFF0F0F0���� $ 	#FFACACAC���� " n�.R�� $��Center=�$
 True��$��Center=�.+�F� $
 grip�������    ��[ �    ��[ `�S `�S �`�S     �ا ��[ �ا ��[ ��ا ��[ �ا ���ا ����ا �������    ����[ ��[ 	8��  $
 Fill=�$
 True��8�� $��1��$
 True��$��Center=� P�  �@J�IsMouseOver$
	 true��. ��� $

 grip��8   � ���� $

 grip��8   � ��P�  �@n�
IsDragging$
	 true��. ��� $

 grip��8   � ���� $

 grip��8   � ��P�  � {�$	 false��. ��� $

 grip��8   � ���� $

 grip��8   � ���� " n�.R�� $��Center=�$
 True��$��Center=�.+�F� $
 grip�������     � �     � `�S ��`�S ���`�S ���ا  � �ا  � ��ا  � �ا �ا ��ا     �         �  � 	8��  $
 Fill=�$
 True��8�� $��1��$
 True��$��Center=� P�  �@J�IsMouseOver$
	 true��. ��� $

 grip��8   � ���� $

 grip��8   � ��P�  �@n�
IsDragging$
	 true��. ��� $

 grip��8   � ���� $

 grip��8   � ��P�  � {�$	 false��. ��� $

 grip��8   � ���� $

 grip��8   � ���� $ 	#FFD6D6D6���� $ 	#FFE7EAEA���� " ��.���  �@�OverridesDefaultStyle$
 true����  � ��
 ���� ��  � |�$ false����  � ��$ false����  � �� �� " ��.R��� #��z��#��z��#��z���� " n�.R�� $��Center=�$
 True��$��Center=�.+�F� $
 grip��d����        �        �ا �ا ��ا     �ا ���ا ����ا �������    ��	8��  $
 Fill=�$
 True��8�� $��1��$
 True��$��Center=� P�  �@J�IsMouseOver$
	 true��. ��� $

 grip��8   � ���� $

 grip��8   � ��P�  �@n�
IsDragging$
	 true��. ��� $

 grip��8   � ���� $

 grip��8   � ��P�  � {�$	 false��. ��� $

 grip��8   � ���� $

 grip��8   � ���� " ��.R��� $ border��#��z��#��z��#��z��$
 True��._��  �� $
��Auto ��� $
��Auto �#��z���� $
��Auto �.+�m� $ TopTick��# z��$��4��$��0,0,0,2q�$	 Top=�$��0��$y�	Collapsed=�m� $ 
BottomTick��# z��$��4��$��0,2,0,0q�$ Bottom=�$��2��$y�	Collapsed=��� $ TrackBackground��8��  $��1q�8�� $	��4.0��$	��5,0q�$��1��$��center=�._��� $��-6,-1q�.+��� $ PART_SelectionRange��#��� ��$	��4.0��$y�Hidden=�]� $ 
PART_Track��$��1�� �� #��Z 8��  �� #��Z 8��  n� $ Thumb��$|�False��$��31��
�������$
 True��$��Center=�$��25������ " n�.R�� #��z��#��z��.+��� $	��bor��#��z��$ 15�� P�  �@n�
IsDragging$
	 true��. ��� $	
 bor�� � �� �	#ca3b9aff   $ 0Q�$ 10Q�$	  0.8Q�$! 	#ca3b9aff�� P�  �@��TickPlacement$	 TopLeft=�. ��� $
 TopTick�� � y�$ Visible=��� $
 Thumb��8   � ���� $
 TrackBackground�� � ��$ 5,2,5,0q�P�  �@��TickPlacement$	 BottomRight=�. ��� $
 
BottomTick�� � y�$ Visible=��� $
 Thumb��8   � ���� $
 TrackBackground�� � ��$ 5,0,5,2q�P�  �@��TickPlacement$
	 Both=�. ��� $
 TopTick�� � y�$ Visible=��� $
 
BottomTick�� � y�$ Visible=�P�  �@��IsSelectionRangeEnabled$
	 true��. ��� $
 PART_SelectionRange�� � y�$ Visible=�P�  �@J�IsKeyboardFocused$
	 true��. ��� $
 Thumb�� � ��
 ��  ��� " n�.R�� $��Center=�$
 True��$��Center=�.+�F� $
 grip���������[ �ا Ӏ�[ �ا `�S `�S �    `�S ��[ ��[ Ӏ�[     ����Ӏ�    ���ا ���ا Ӏ��ا ��[ �ا ��[ �ا 	8��  $
 Fill=�8��  P�  �@J�IsMouseOver$
	 true��. ��� $

 grip��8   � ���� $

 grip��8   � ��P�  �@n�
IsDragging$
	 true��. ��� $

 grip��8   � ���� $

 grip��8   � ��P�  � {�$	 false��. ��� $

 grip��8   � ���� $

 grip��8   � ���� " n�.R�� $��Center=�$
 True��$��Center=�.+�F� $
 grip������� � �ا � � �ا ��`�S ��`�S Ӏ�`�S  �  � � �     �        �ا �ا �    �ا  � �ا  � �ا 	8��  $
 Fill=�8��  P�  �@J�IsMouseOver$
	 true��. ��� $

 grip��8   � ���� $

 grip��8   � ��P�  �@n�
IsDragging$
	 true��. ��� $

 grip��8   � ���� $

 grip��8   � ��P�  � {�$	 false��. ��� $

 grip��8   � ���� $

 grip��8   � ���� " n�.R�� $��Center=�$
 True��$��Center=�.+�F� $
 grip��+���� �  � ѠI � ѠI�y� � � �y� 	8��  $
 Fill=�8��  P�  �@J�IsMouseOver$
	 true��. ��� $

 grip��8   � ���� $

 grip��8   � ��P�  �@n�
IsDragging$
	 true��. ��� $

 grip��8   � ���� $

 grip��8   � ��P�  � {�$	 false��. ��� $

 grip��8   � ���� $

 grip��8   � ���� " ��.R��� $ border��#��z��#��z��#��z��$
 True��._�� " �� $
��Auto ��� #��z��$
��Auto ��� $
��Auto �.+�m� $ TopTick��$��0��# z��$��0,0,2,0q�$
 Left=�$y�	Collapsed=�$��4��m� $ 
BottomTick��$��2��# z��$��2,0,0,0q�$ Right=�$y�	Collapsed=�$��4���� $ TrackBackground��8��  $��1q�8�� $��1��$��center=�$	��0,5q�$	��4.0��._��� $��-1,-6q�.+��� $ PART_SelectionRange��#��� ��$y�Hidden=�$	��4.0��]� $ 
PART_Track��$��1�� �� #��Z 8��  �� #��Z 8��  n� $ Thumb��$|�False��$��11��$
 True��8�� $	��Top=�$��18�� P�  �@��TickPlacement$	 TopLeft=�. ��� $
 TopTick�� � y�$ Visible=��� $
 Thumb��8   � ���� $
 TrackBackground�� � ��$ 2,5,0,5q�P�  �@��TickPlacement$	 BottomRight=�. ��� $
 
BottomTick�� � y�$ Visible=��� $
 Thumb��8   � ���� $
 TrackBackground�� � ��$ 0,5,2,5q�P�  �@��TickPlacement$
	 Both=�. ��� $
 TopTick�� � y�$ Visible=��� $
 
BottomTick�� � y�$ Visible=�P�  �@��IsSelectionRangeEnabled$
	 true��. ��� $
 PART_SelectionRange�� � y�$ Visible=�P�  �@J�IsKeyboardFocused$
	 true��. ��� $
 Thumb�� � ��
 ��  ��� " ��.���  �@��IsPressAndHoldEnabled$ false����  � ��
 ���� ��  � ��
 ���� ��  � ��8   ��  � ��8  # P�  �@��Orientation$	 Vertical=�. ���  � ��8  .��(� .���� -   $	��400��$��60�����#252526$��1q����	#50ffffff   $ 0Q�$ 5Q�$	  0.5Q�$! White��._�� $	��400��$��60�� �� $��0 ��� $��60 �.+��� -   $ ��Name$$ lblName��$��16(�
�������% �� Padding$% 12q�音量调节�� $��1��.+��� -   & ��Name$& slider��' �� IsMoveToPointEnabled$
' True��( �� Left$( 10��$��60��) � Maximum$	) 100Q�$��60��$	��300��$��Center=�$��Center=�#���  * � SmallChange$* 1Q�+ �� AutoToolTipPlacement$+ BottomRight=��� -   $$ sliderValue��$	( 315��
�������$��30(�$��85��$��60��$% 12q�, �� TextAlignment$, Center=�0