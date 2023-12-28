pk12util -d sql:$HOME/.pki/nssdb -i presencelight.pfx

certutil -d sql:$HOME/.pki/nssdb -A -t "P,," -n 'presencelight' -i presencelight.crt