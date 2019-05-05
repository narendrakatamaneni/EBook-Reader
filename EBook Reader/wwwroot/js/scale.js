
        function go() {
            document.getElementById('frame').src = document.getElementById("usrin").value;
        }

        /*---- make picture bigger ----*/

        function biggerImage(id) {
            let pict = document.getElementById(id);
            pict.width = 1.2 * pict.width;
            pict.height =1.6*pict.height;
        }
        /*---- make picture smaller ----*/

        function smallerImage(id) {
            let pict = document.getElementById(id);

            pict.width = pict.width / 1.2;
            pict.height = pict.height / 1.6;
        }

         /*---- hide iframe ----*/

        function hideImage(id) {
            let pict = document.getElementById(id);
           // let btn = document.getElementById('hidebtn');
            pict.style.display = (pict.style.display == 'block') ? 'none' : 'block';
        }
