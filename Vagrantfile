# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  config.vm.box = "pgdevbox"
  config.vm.box_url = "http://pgdevbox.com/pgdev.box"
  config.vm.network :forwarded_port, guest: 5432, host: 5433
  config.vm.provision "shell", inline: <<-SHELL
    sudo apt-get install -y lzop
    #lzop -cd /vagrant/db_backups/awisbnpt.2014-12-29.sql.lzo | psql --user vagrant --db vagrant
  SHELL
end
