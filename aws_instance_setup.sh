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

# everything will be stored in /app
# optionally mount a separate volume here
sudo mkdir /app
sudo chown ec2-user:ec2-user /app
cd /app

# install required packages
sudo yum update -y
sudo yum install -y git docker httpd mod_ssl

# download and install docker-compose manually from releases on their github
# link: https://github.com/docker/compose/releases
# this guide will assume you placed the installed binary in /usr/local/bin/docker-compose

# enable packages which are needed to install certbot
sudo wget -r --no-parent -A 'epel-release-*.rpm' https://dl.fedoraproject.org/pub/epel/7/x86_64/Packages/e/
sudo rpm -Uvh dl.fedoraproject.org/pub/epel/7/x86_64/Packages/e/epel-release-*.rpm
sudo yum-config-manager --enable epel*
sudo rm -rf dl.fedoraproject.org/

# install certbot
sudo amazon-linux-extras install epel -y
sudo yum install -y certbot python2-certbot-apache

# now create ut4masterserver.conf in /etc/httpd/conf.d/ and put in the following:
# <VirtualHost *:80>
# 	DocumentRoot "/var/www/html"
# 	ServerName "<PUT_WEBSITE_DOMAIN_NAME_HERE>"
#	ProxyPreserveHost On
#	ProxyPass / http://127.0.0.1:5001/
#	ProxyPassReverse / http://127.0.0.1:5001/
# </VirtualHost>
# <VirtualHost *:80>
# 	ServerName "<PUT_API_DOMAIN_NAME_HERE>"
#	ProxyPreserveHost On
#	ProxyPass / http://127.0.0.1:5000/
#	ProxyPassReverse / http://127.0.0.1:5000/
# </VirtualHost>

# make sure httpd (apache) daemon is running at all times
sudo systemctl enable httpd & sudo systemctl restart httpd

# install 2 ssl certificates. one for WEBSITE domain and one for API domain.
# run certbot and follow it's instructions, then make sure it installed the obtained certificates.
sudo certbot

# get source code of master server (use your own fork if you have a custom version)
git clone https://github.com/timiimit/UT4MasterServer

# make sure docker daemon is running at all times
sudo systemctl enable docker && sudo systemctl restart docker

# start all containers required to run the master server
sudo /usr/local/bin/docker-compose -f UT4MasterServer/docker-compose.yml up -d

# make sure to take care of /app/db as that directory contains the database