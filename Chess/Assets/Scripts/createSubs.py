import json
import sys
import time

j = json.load(open(sys.argv[1], 'r'))
data = j['data']
last_time = -1
index = 1
first_time = None
last_good_left = 'Invalid'
last_good_right = 'Invalid'

for d in data:
    elapsed_seconds = d['systemTimeStamp'] / 1000000
    if not first_time:
        first_time = elapsed_seconds
    minutes, seconds = divmod(elapsed_seconds - first_time, 60)
    start_time = "00:{minutes:0>2}:{seconds:0>2}".format(minutes=int(minutes),
                                                         seconds=int(seconds))
    end_time = "00:{minutes:0>2}:{seconds:0>2}".format(minutes=int(minutes),
                                                       seconds=int(seconds) + 1)

    if start_time != last_time:
        left = d['leftEye']['pupilDiameter']
        right = d['rightEye']['pupilDiameter']
        if d['leftEye']['isPupilDiameterValid']:
            last_good_left = left
        else:
            left = last_good_left
        if d['rightEye']['isPupilDiameterValid']:
            last_good_right = right
        else:
            right = last_good_right

        display_data = f"{right:2f}, {left:2f}"

        message = f'<font color="#ffffff" face="Arial" size="-1">{display_data}</font>'
        #message = f'{display_data}'
        print(index)
        print(f"{start_time},250 --> {end_time},200")
        print(message)
        print('')
        index += 1
        last_time = start_time