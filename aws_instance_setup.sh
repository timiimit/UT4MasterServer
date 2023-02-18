exit

###################################
# DO NOT RUN THIS SCRIPT DIRECTLY #
# IT IS ONLY MEANT AS A GUIDE TO  #
# PREPARE EVERYTHING NECCESSERY   #
###################################

# if you have SSH terminal problems you need to upload your
# terminal information to the host.
# to do this:
#  - locally create 'terminfo' file with 'infocmp > terminfo' command
#  - upload the file to target host
#  - run 'tic terminfo' on the host in the same directory as the file resides

WEBSITE_DOMAIN_NAME=
API_DOMAIN_NAME=
CERTIFICATE_REGISTRATION_EMAIL=

# everything will be stored in /app
# optionally mount a separate volume here
sudo mkdir /app
sudo chown ec2-user:ec2-user /app
cd /app

# install required packages
sudo yum update -y
sudo yum install -y git docker httpd mod_ssl

# download and install docker-compose manually from their github
sudo curl -SL https://github.com/docker/compose/releases/download/v2.15.0/docker-compose-linux-x86_64 -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# make docker-compose command available
sudo ln -s /usr/local/bin/docker-compose /usr/bin/docker-compose

# make sure docker daemon is running at all times
sudo systemctl enable docker && sudo systemctl restart docker

# install epel repository which contains certbot
sudo yum install -y https://dl.fedoraproject.org/pub/epel/epel-release-latest-7.noarch.rpm

# install certbot
sudo yum install -y certbot python2-certbot-apache

# now create ut4masterserver.conf in /etc/httpd/conf.d/ and put in the following:
cat >ut4masterserver.conf << EOF
<VirtualHost *:80>
    DocumentRoot "/var/www/html"
    ServerName "$WEBSITE_DOMAIN_NAME"
    ProxyPreserveHost On
    ProxyPass / http://127.0.0.1:5001/
    ProxyPassReverse / http://127.0.0.1:5001/
</VirtualHost>
<VirtualHost *:80>
    ServerName "$API_DOMAIN_NAME"
    ProxyPreserveHost On
    ProxyPass / http://127.0.0.1:5000/
    ProxyPassReverse / http://127.0.0.1:5000/
</VirtualHost>
EOF
sudo cp ut4masterserver.conf /etc/httpd/conf.d/ut4masterserver.conf
rm ut4masterserver.conf

# make sure httpd (apache) daemon is running at all times
sudo systemctl enable httpd & sudo systemctl restart httpd

# install 2 ssl certificates. one for WEBSITE domain and one for API domain.
# run certbot and follow it's instructions, then make sure it installed the obtained certificates.
#sudo certbot

# you can run this automated command instead of manually entering fields
sudo certbot -d $API_DOMAIN_NAME -d $WEBSITE_DOMAIN_NAME --email $CERTIFICATE_REGISTRATION_EMAIL --non-interactive --apache --agree-tos

# get source code of master server (use your own fork if you have a custom version)
git clone https://github.com/timiimit/UT4MasterServer

# start all containers required to run the master server
sudo /usr/local/bin/docker-compose -f UT4MasterServer/docker-compose.yml up -d

# make sure to take care of /app/db as that directory contains the database
