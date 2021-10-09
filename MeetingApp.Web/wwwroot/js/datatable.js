$(document).ready(function () {
    var counter = 0;

    $("#addrow").on("click", function () {
        var newRow = $("<tr>");
        var cols = "";

        cols += '<td class="col-sm-4"><div class="input-group date" id = "datepicker1"><input asp-for="date" type="text" class="form-control" name="_meetingDate" value="@DateTime.Now.ToString(" yyyy-MM-ddThh:mm")" /><span class="input-group-addon"><span class="glyphicon glyphicon-calendar"></span></span></div ></td >';
        cols += '<td class="col-sm-4"><div class="input-group date" id = "datepicker2"><input asp-for="date" type="text" class="form-control" name="_meetingDate" value="@DateTime.Now.ToString(" yyyy-MM-ddThh:mm")" /><span class="input-group-addon"><span class="glyphicon glyphicon-calendar"></span></span></div ></td >';

        cols += '<td><input type="button" class="ibtnDel btn btn-md btn-danger "  value="Delete"></td>';
        newRow.append(cols);
        $("table.order-list").append(newRow);
        counter++;
    });



    $("table.order-list").on("click", ".ibtnDel", function (event) {
        $(this).closest("tr").remove();
        counter -= 1
    });


});



function calculateRow(row) {
    var price = +row.find('input[name^="price"]').val();

}

function calculateGrandTotal() {
    var grandTotal = 0;
    $("table.order-list").find('input[name^="price"]').each(function () {
        grandTotal += +$(this).val();
    });
    $("#grandtotal").text(grandTotal.toFixed(2));
}