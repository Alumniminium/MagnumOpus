#!/usr/bin/env bash
sudo iptables -t nat -A OUTPUT -p tcp --dport 9958 -j DNAT --to-destination 192.168.0.10:9958
sudo iptables -t nat -A OUTPUT -p tcp --dport 5816 -j DNAT --to-destination 192.168.0.10:5816
ssh -R 5816:127.0.0.1:5816 -R 9958:127.0.0.1:9958 alu@192.168.0.2
