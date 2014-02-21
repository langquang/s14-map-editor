- Cấu trúc project
	+ *.s14map: xml lưu thông tin map
	+ folder "data": chức các file jpg, png, pixma sử dụng trong map
	*** folder "data" là DefaultcontentRootFolder của project. nên đặt "data" cùng cấp với *.s14map
để không cần thay đổi DefaultcontentRootFolder khi dùng trên PC khác. (Tools/Settings)


- Setup
	+ Run file "S14Editor.exe" trong folder tool.
	+ run "xnafx31_redist" để cài XNA (nếu chưa cài)

	
- Example
	+ Mở editor
	+ Open "projects/example/Map_example"
	
- Đinh dạnh hổ trợ
	+ png : png file
	+ frame: pixma frame
	+ anim : pixma animation
	+ mask
		# Restricted Area: file png có tên chứa chuỗi "mask", Origin tại Top - Center
		# background: file png có tên chứa chuỗi "map", Origin tại Top - Left
	
	
- cấu trúc Layer (TinsBQ đề nghị - edit sau!!!)
	+ Tiles: chứa tileset hoặc backgroundPNG (frame, mask)
	+ Modules: chứa module lớn : cỏ, núi, đất ( frame)
	+ Decos: chứa animation trang trí (anim)
	+ Restricted Area: mask tile không thể di chuyển (mask)
	+ World Object: object trong game (anim)
	+ Items: vị trí xuất hiện các item (anim)
	
	
- Hướng dẫn: Chường trinh được viết dựa trên "GLEED2D" nên chức năng tương tự.
link: https://www.youtube.com/watch?v=FiUdJYFEfIs

- Liên hệ: TinBQ2
	
	


