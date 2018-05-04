
with open('values.txt', 'w') as f_out, open('trimmed_without_repeating_points.txt') as f_in:
    count = 0
    for line in f_in:
        count = count + 1
        if line[-2] == ',':
            a = line[:-2]
            line = a + line[-1]
        f_out.write(line)
print('done')
